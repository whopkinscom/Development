using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Principal;
using Moonrise.Utils.Standard.Config;
using NDesk.Options;
using NDesk.Options.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EncryptAppSettings
{
    /// <summary>
    ///     This is a pretty simple program so everything happens within this class!
    /// </summary>
    internal class Program
    {
        public struct CommandLineOptions
        {
            public Switch Decrypt { get; set; }
            public Variable<string> SettingsFile { get; set; }
            public Variable<string> AffectedSettingsCsv { get; set; }
            public Variable<string> AffectedSettingsFile { get; set; }
        }

        /// <summary>
        ///     Represents metadata of the encryption status of the settings file
        /// </summary>
        public class EncryptedBy
        {
            /// <summary>
            ///     The timestamp of when the settings file was last decrypted.
            /// </summary>
            public DateTimeOffset DecryptedAt { get; set; }

            /// <summary>
            ///     The timestamp of when the settings file was last encrypted.
            /// </summary>
            public DateTimeOffset EncryptedAt { get; set; }

            /// <summary>
            ///     The name of the machine that the encryption can only be decrypted on
            /// </summary>
            public string MachineName { get; set; }

            /// <summary>
            ///     This can only be decrypted by the encrypting user on the encrypting machine. Used as an authorisation check!
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            ///     Name of the user that encrypted elements of the settings file. This is encrypted at the machine level and can only
            ///     be accessed on the same machine that performed the encryption.
            /// </summary>
            public string Username { get; set; }
        }

        private static CommandLineOptions commandLine;

        private static readonly List<string> settingsToEncrypt = new List<string>();

        /// <summary>
        ///     Adds the encryption metadata.
        /// </summary>
        /// <exception cref="EncryptAppSettings.EncryptAppException"></exception>
        private static void AddEncryptionMetadata()
        {
            try
            {
                DateTimeOffset previouslyDecryptedAt;
                Settings.Application.Read("EncryptedBy:DecryptedAt", out previouslyDecryptedAt);
                EncryptedBy encryptedBy = new EncryptedBy
                                          {
                                              Username = WindowsIdentity.GetCurrent().Name,
                                              Name = WindowsIdentity.GetCurrent().Name,
                                              MachineName = Environment.MachineName,
                                              EncryptedAt = DateTimeOffset.Now,
                                              DecryptedAt = previouslyDecryptedAt
                                          };

                Settings.Application.Write("EncryptedBy", encryptedBy);
                Settings.Application.RefreshAnyCaches();
                Settings.Application.SettingsEncryptor = new DpApiSettingsEncryptor(DpApiSettingsEncryptor.ProtectionScope.Machine);
                Settings.Application.Write("EncryptedBy:Username", encryptedBy.Username, true);
                Settings.Application.SettingsEncryptor = new DpApiSettingsEncryptor(DpApiSettingsEncryptor.ProtectionScope.User);
                Settings.Application.Write("EncryptedBy:Name", encryptedBy.Name, true);
            }
            catch (Exception excep)
            {
                throw new EncryptAppException(EncryptAppExceptionReason.UnknownException, nameof(AddEncryptionMetadata), excep.Message);
            }
        }

        /// <summary>
        ///     Checks the encryption metadata to determine if the user is the user that encrypted the settings.
        /// </summary>
        /// <exception cref="EncryptAppSettings.EncryptAppException"></exception>
        private static void CheckEncryptionMetadata()
        {
            try
            {
                string machineName;
                Settings.Application.Read("EncryptedBy:MachineName", out machineName);
                Settings.Application.SettingsEncryptor = new DpApiSettingsEncryptor(DpApiSettingsEncryptor.ProtectionScope.Machine);
                try
                {
                    string encryptedBy;
                    Settings.Application.Read("EncryptedBy:Username", out encryptedBy);
                    Settings.Application.SettingsEncryptor = new DpApiSettingsEncryptor(DpApiSettingsEncryptor.ProtectionScope.User);

                    string name;

                    try
                    {
                        Settings.Application.Read("EncryptedBy:Name", out name);
                    }
                    catch (Exception)
                    {
                        throw new EncryptAppException(EncryptAppExceptionReason.UnauthorisedUser, encryptedBy);
                    }

                    try
                    {
                        Settings.Application.Write("EncryptedBy:DecryptedAt", DateTimeOffset.Now);
                    }
                    catch (SettingsException excep)
                    {
                        if (excep.ReasonCode == SettingsExceptionReason.InvalidKey)
                            throw new EncryptAppException(EncryptAppExceptionReason.FileNotPreviouslyEncrypted);

                        throw new EncryptAppException(EncryptAppExceptionReason.UnknownException, nameof(CheckEncryptionMetadata), excep.Message);
                    }
                }
                catch (EncryptAppException)
                {
                    throw;
                }
                catch (Exception)
                {
                    throw new EncryptAppException(EncryptAppExceptionReason.UnauthorisedMachine, machineName);
                }
            }
            catch (EncryptAppException)
            {
                throw;
            }
            catch (SettingsException excep)
            {
                if (excep.ReasonCode == SettingsExceptionReason.InvalidKey)
                    throw new EncryptAppException(EncryptAppExceptionReason.FileNotPreviouslyEncrypted);

                throw new EncryptAppException(EncryptAppExceptionReason.UnknownException, nameof(CheckEncryptionMetadata), excep.Message);
            }
            catch (Exception excep)
            {
                throw new EncryptAppException(EncryptAppExceptionReason.UnknownException, nameof(CheckEncryptionMetadata), excep.Message);
            }
        }

        /// <summary>
        ///     Encrypts or decrypt specified settings.
        /// </summary>
        /// <param name="encryptSetting">Indicates if the settings are to be encrypted or decrypted</param>
        private static void EncryptOrDecryptSettings(bool encryptSetting)
        {
            try
            {
                // En/Decryption is always done with Machine level protection scope. i.e. It has to be done on the machine concerned
                Settings.Application.SettingsEncryptor = new DpApiSettingsEncryptor(DpApiSettingsEncryptor.ProtectionScope.Machine);

                foreach (string setting in settingsToEncrypt)
                {
                    string trimmedSetting = setting.Trim();

                    try
                    {
                        object valueAsObject = null;

                        // Note that we read the value to en/decrypt as a JToken - this assumes we are, which we are, dealing with a JSON settings file!
                        try
                        {
                            if (!Settings.Application.Read(trimmedSetting, out JToken value))
                                throw new EncryptAppException(EncryptAppExceptionReason.SettingNotFound, trimmedSetting, commandLine.SettingsFile);

                            valueAsObject = value;
                        }
                        catch (JsonReaderException)
                        {
                            // If a JToken doesn't work, the setting will be a string.
                            if (!Settings.Application.Read(trimmedSetting, out string value))
                                throw new EncryptAppException(EncryptAppExceptionReason.SettingNotFound, trimmedSetting, commandLine.SettingsFile);

                            valueAsObject = value;
                        }

                        Settings.Application.Write(trimmedSetting, valueAsObject, encryptSetting);
                    }
                    catch (SettingsException excep)
                    {
                        if (excep.ReasonCode == SettingsExceptionReason.InvalidKey)
                            throw new EncryptAppException(EncryptAppExceptionReason.SettingNotFound, trimmedSetting, commandLine.SettingsFile);
                    }
                }
            }
            catch (Exception excep)
            {
                throw new EncryptAppException(EncryptAppExceptionReason.UnknownException, nameof(EncryptOrDecryptSettings), excep.Message);
            }
        }

        /// <summary>
        ///     Main entry point for the program!
        /// </summary>
        /// <param name="args">The arguments.</param>
        private static void Main(string[] args)
        {
            try
            {
                try
                {
                    if (ProcessArguments(args))
                    {
                        PrepareConfig();

                        if (!commandLine.Decrypt)
                        {
                            AddEncryptionMetadata();
                        }
                        else
                        {
                            CheckEncryptionMetadata();
                        }

                        EncryptOrDecryptSettings(!commandLine.Decrypt);
                    }
                }
                catch (EncryptAppException)
                {
                    throw;
                }
                catch (Exception excep)
                {
                    throw new EncryptAppException(EncryptAppExceptionReason.UnknownException, nameof(Main), excep.Message);
                }
            }
            catch (EncryptAppException excep)
            {
                Console.WriteLine(excep.Message);
                Environment.Exit(excep.ErrorCode);
            }
        }


        /// <summary>
        /// [{ENC]}
        /// </summary>
        /// <exception cref="EncryptAppSettings.EncryptAppException">PrepareConfig</exception>
        private static void PrepareConfig()
        {
            try
            {
                JsonConfigSettingsProvider.Config config = new JsonConfigSettingsProvider.Config
                                                           {
                                                               ApplicationSettingsFilename = Path.GetFileName(commandLine.SettingsFile),
                                                               ApplicationSettingsFolder = Path.GetDirectoryName(commandLine.SettingsFile)
                                                           };
                Settings.Application.SettingsProvider = new JsonConfigSettingsProvider(config);
            }
            catch (Exception excep)
            {
                throw new EncryptAppException(EncryptAppExceptionReason.UnknownException, nameof(PrepareConfig), excep.Message);
            }
        }

        /// <summary>
        ///     Processes the arguments, utilising NDesk.Options.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>True if the command line was successfully processed</returns>
        private static bool ProcessArguments(string[] args)
        {
            try
            {
                bool retVal;

                // Create a commandline option set for processing command line options
                RequiredValuesOptionSet options = new RequiredValuesOptionSet();

                commandLine.SettingsFile = options.AddRequiredVariable<string>("f|File", "Settings file to encrypt/decrypt");
                commandLine.Decrypt = options.AddSwitch("d|Decrypt", "Indicates that settings are to be decrypted - default is to encrypt");
                commandLine.AffectedSettingsCsv =
                    options.AddVariable<string>("s|Settings", "Comma seperated list of individual settings to encrypt/decrypt");
                commandLine.AffectedSettingsFile = options.AddVariable<string>("sf|SettingsFile",
                                                                               "Filepath to the file containing line seperated settings to encrypt/decrypt");

                // Process the command line arguments
                ConsoleManager consoleOptionsManager = new ConsoleManager("Settings Encryptor", options);
                retVal = consoleOptionsManager.TryParseOrShowHelp(Console.Out, args);

                if (retVal)
                {
                    if (commandLine.AffectedSettingsCsv.Value != null)
                    {
                        settingsToEncrypt.AddRange(commandLine.AffectedSettingsCsv.Value.Split(','));
                    }
                    else if (commandLine.AffectedSettingsFile.Value == null)
                        throw new EncryptAppException(EncryptAppExceptionReason.NoSettingsSpecified);
                    else
                    {
                        if (!File.Exists(commandLine.AffectedSettingsFile))
                            throw new EncryptAppException(EncryptAppExceptionReason.SettingsInputFileNotFound,
                                                          commandLine.AffectedSettingsFile.Value);

                        // Read the list of settings from the settings file
                        string[] readSettings = File.ReadAllLines(commandLine.AffectedSettingsFile);

                        foreach (string readSetting in readSettings)
                        {
                            // Be quite forgiving about how the settings are listed.
                            settingsToEncrypt.AddRange(readSetting.Trim(' ', '"', '\'', ',').Split(','));
                        }
                    }
                }

                return retVal;
            }
            catch (EncryptAppException)
            {
                throw;
            }
            catch (OptionException excep)
            {
                throw new EncryptAppException(EncryptAppExceptionReason.CommandLineOptionException, excep.Message);
            }
            catch (Exception excep)
            {
                throw new EncryptAppException(EncryptAppExceptionReason.UnknownException, nameof(ProcessArguments), excep.Message);
            }
        }
    }
}
