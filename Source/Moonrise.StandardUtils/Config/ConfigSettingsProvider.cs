// <copyright file="ConfigSettingsProvider.cs" company="Moonrise Media Ltd.">
// Originally written by WillH - with any acknowledgements as required. Once checked in to your version control you have full rights except for selling the source!
// </copyright>

using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.Configuration;
using Moonrise.Utils.Standard.Files;
using Newtonsoft.Json;

namespace Moonrise.Utils.Standard.Config
{
    /// <summary>
    ///     Uses the default .Net config files, be they app.config or the user.config buried deep wherever!
    /// </summary>
    /// <seealso cref="Moonrise.Utils.Standard.Config.ISettingsProvider" />
    public class ConfigSettingsProvider : ISettingsProvider
    {
        private static Dictionary<string, string> _userSettings = new Dictionary<string, string>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConfigSettingsProvider" /> class.
        /// </summary>
        /// <param name="settingsFilename">The settings filename.</param>
        public ConfigSettingsProvider(string settingsFilename = "UserSettings.json")
        {
            SettingsFilename = settingsFilename;
        }

        /// <summary>
        ///     Only used by the <see cref="Settings" /> class, but indicates if this provider has read its cache yet.
        /// </summary>
        public bool CacheRead { get; set; }

        /// <summary>
        ///     The settings filename.
        /// </summary>
        public string SettingsFilename { get; set; }

        /// <summary>
        ///     Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        ///     A new object that is a copy of this instance.
        /// </returns>
        public object Clone()
        {
            return new ConfigSettingsProvider(SettingsFilename);
        }

        /// <summary>
        ///     Encrypt the setting at the settingContext using the settingsEncryptor
        /// </summary>
        /// <param name="settingContext">context for the setting. e.g. "Root:Parent:Child:etc"</param>
        /// <param name="settingsEncryptor">The encryptor to use</param>
        /// <param name="type">The type of the setting</param>
        public bool EncryptSetting(string settingContext, ISettingsEncryptor settingsEncryptor, SettingType type)
        {
            bool retVal = false;

            return retVal;
        }

        /// <summary>
        ///     Write out the current settings
        /// </summary>
        /// <param name="type">The type of setting.</param>
        public void Flush(SettingType type)
        {
            if (type == SettingType.User)
            {
                WriteUserSettingFile();
            }
        }

        /// <summary>
        ///     Reads an application or user setting.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="type">The type of setting.</param>
        /// <returns>
        ///     The value.
        /// </returns>
        public string ReadSetting(string key, SettingType type = SettingType.Application)
        {
            if (type == SettingType.Application)
            {
                return ConfigurationManager.AppSettings[key];
            }

            if (type == SettingType.User)
            {
                return ReadUserSetting(key);
            }

            return string.Empty;
        }

        /// <summary>
        ///     Refreshes any caches that might be in use.
        /// </summary>
        public void RefreshAnyCaches(SettingType type)
        {
            if (type == SettingType.User)
            {
                ReadUserSettingsFile();
            }
            else
            {
                ConfigurationManager.RefreshSection("AppSettings");
            }
        }

        /// <summary>
        ///     Writes the application setting.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="objval">The value as an object - Use the most appropriate.</param>
        /// <param name="type">The type of setting.</param>
        public void WriteSetting(string key, string value, object objval, SettingType type = SettingType.Application)
        {
            if (type == SettingType.Application)
            {
                WriteApplicationSetting(key, value);
            }
            else
            {
                WriteUserSetting(key, value);
            }
        }

        /// <summary>
        ///     Opens the configuration file, determining between a web and app config.
        /// </summary>
        /// <returns>The opened config file</returns>
        private static Configuration OpenConfigFile()
        {
            Configuration retVal;

            if ((HttpContext.Current != null) && !HttpContext.Current.Request.PhysicalPath.Equals(string.Empty))
            {
                retVal = WebConfigurationManager.OpenWebConfiguration("~");
            }
            else
            {
                retVal = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            }

            return retVal;
        }

        /// <summary>
        ///     Reads the user setting from the locally stored dictionary.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The value</returns>
        private static string ReadUserSetting(string key)
        {
            string retVal;
            if (_userSettings.TryGetValue(key, out retVal))
            {
                return retVal;
            }

            return string.Empty;
        }

        /// <summary>
        ///     Writes the application setting into the application config file.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        private static void WriteApplicationSetting(string key, string value)
        {
            Configuration confFile = OpenConfigFile();
            confFile.AppSettings.Settings.Add(key, value);
            confFile.Save(ConfigurationSaveMode.Minimal);
            ConfigurationManager.RefreshSection(confFile.AppSettings.SectionInformation.Name);
        }

        /// <summary>
        ///     Reads the user setting file from the roaming user path
        /// </summary>
        private void ReadUserSettingsFile()
        {
            string jsonedDict = FileUtils.ReadFile(FileUtils.RoamingUserApplicationPath(), SettingsFilename);

            if (!string.IsNullOrEmpty(jsonedDict))
            {
                _userSettings = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonedDict);
            }
        }

        /// <summary>
        ///     Writes the user setting into the local dictionary and then updates the entire dictionary in the settings file in
        ///     the roaming user path
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        private void WriteUserSetting(string key, string value)
        {
            _userSettings[key] = value;
            WriteUserSettingFile();
        }

        /// <summary>
        ///     Writes the user setting file into the roaming user path
        /// </summary>
        private void WriteUserSettingFile()
        {
            string jsonedDict = JsonConvert.SerializeObject(_userSettings);
            FileUtils.WriteFile(FileUtils.RoamingUserApplicationPath(), SettingsFilename, jsonedDict);
        }

        //    ConsoleSection currentSection = null;
        //    string sectionName = "consoleSection";
        //    // configuration file.
        //    // Define the custom section to add to the

        //// ConfigurationUserLevel userLevel) method to 
        //// This function uses the OpenExeConfiguration(
        //// with the application.

        //// Get the roaming configuration file associated 
        //// get the configuration file.
        //// It also creates a custom ConsoleSection and 
        //// sets its ConsoleEment BackgroundColor and
        //// ForegroundColor properties to blue and yellow
        //// respectively. Then it uses these properties to
        //// set the console colors.  

        //public static void GetRoamingConfiguration()

        //{

        //    // Get the roaming configuration 
        //    // that applies to the current user.
        //    Configuration roamingConfig =
        //      ConfigurationManager.OpenExeConfiguration(
        //       ConfigurationUserLevel.PerUserRoaming);

        //    // Map the roaming configuration file. This
        //    // enables the application to access 
        //    // the configuration file using the
        //    // System.Configuration.Configuration class
        //    ExeConfigurationFileMap configFileMap =
        //      new ExeConfigurationFileMap();
        //    configFileMap.ExeConfigFilename =
        //      roamingConfig.FilePath;

        //    // Get the mapped configuration file.
        //    Configuration config =
        //      ConfigurationManager.OpenMappedExeConfiguration(
        //        configFileMap, ConfigurationUserLevel.None);

        //    try
        //    {
        //        currentSection =
        //             (ConsoleSection)config.GetSection(
        //               sectionName);

        //        // Synchronize the application configuration
        //        // if needed. The following two steps seem
        //        // to solve some out of synch issues 
        //        // between roaming and default
        //        // configuration.
        //        config.Save(ConfigurationSaveMode.Modified);

        //        // Force a reload of the changed section, 
        //        // if needed. This makes the new values available 
        //        // for reading.
        //        ConfigurationManager.RefreshSection(sectionName);

        //        if (currentSection == null)
        //        {
        //            // Create a custom configuration section.
        //            currentSection = new ConsoleSection();

        //            // Define where in the configuration file 
        //            // hierarchy the associated 
        //            // configuration section can be declared.
        //            // The following assignment assures that 
        //            // the configuration information can be 
        //            // defined in the user.config file in the 
        //            // roaming user directory. 
        //            currentSection.SectionInformation.AllowExeDefinition =
        //              ConfigurationAllowExeDefinition.MachineToLocalUser;

        //            // Allow the configuration section to be 
        //            // overridden by lower-level configuration files.
        //            // This means that lower-level files can contain
        //            // the section (use the same name) and assign 
        //            // different values to it as done by the
        //            // function GetApplicationConfiguration() in this
        //            // example.
        //            currentSection.SectionInformation.AllowOverride =
        //              true;

        //            // Store console settings for roaming users.
        //            currentSection.ConsoleElement.BackgroundColor =
        //                ConsoleColor.Blue;
        //            currentSection.ConsoleElement.ForegroundColor =
        //                ConsoleColor.Yellow;

        //            // Add configuration information to 
        //            // the configuration file.
        //            config.Sections.Add(sectionName, currentSection);
        //            config.Save(ConfigurationSaveMode.Modified);
        //            // Force a reload of the changed section. This 
        //            // makes the new values available for reading.
        //            ConfigurationManager.RefreshSection(
        //              sectionName);
        //        }
        //    }
        //    catch (ConfigurationErrorsException e)
        //    {
        //        Console.WriteLine("[Exception error: {0}]",
        //            e.ToString());
        //    }

        //    // Set console properties using values
        //    // stored in the configuration file.
        //    Console.BackgroundColor =
        //      currentSection.ConsoleElement.BackgroundColor;
        //    Console.ForegroundColor =
        //      currentSection.ConsoleElement.ForegroundColor;
        //    // Apply the changes.
        //    Console.Clear();

        //    // Display feedback.
        //    Console.WriteLine();
        //    Console.WriteLine(
        //      "Using OpenExeConfiguration(ConfigurationUserLevel).");
        //    Console.WriteLine(
        //        "Configuration file is: {0}", config.FilePath);
        //}
    }
}
