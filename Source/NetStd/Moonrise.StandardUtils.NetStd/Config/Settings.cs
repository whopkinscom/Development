#region Apache-v2.0

//    Copyright 2017 Will Hopkins - Moonrise Media Ltd.
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
//using Moonrise.Utils.Standard.Extensions;
using Newtonsoft.Json;

namespace Moonrise.Utils.Standard.Config
{
    /// <summary>
    ///     Makes working with .config files a little easier!
    /// </summary>
    public class Settings
    {
        /// <summary>
        ///     Indicates what 'attribute' of an enum to write, if the value is not in the config file
        /// </summary>
        public enum WhatAttributeOfEnum
        {
            /// <summary>
            ///     The numeric value, e.g. 0, 1, 2, ...
            /// </summary>
            NumericValue,

            /// <summary>
            ///     The enum value, e.g. Red, Blue, Green
            /// </summary>
            EnumValue,

            /// <summary>
            ///     The description, e.g. "Red House", "Blue House", "Green House"
            /// </summary>
            Description,

            /// <summary>
            ///     The modified description, i.e. the Description that has been modified at run-time, say for translations.
            /// </summary>
            ModifiedDescription
        }

        /// <summary>
        ///     The settings instance type
        /// </summary>
        private readonly SettingType _type;

        private readonly ISettingsProvider settingsProvider;

        /// <summary>
        ///     The original provider - This is the one used to clone providers for other threads
        /// </summary>
        private ISettingsProvider _originalProvider;

        /// <summary>
        ///     The settings provider is stored as a per-thread singleton
        /// </summary>
        private ThreadLocal<ISettingsProvider> _settingsProvider;

        /// <summary>
        ///     Prevents a default instance of the <see cref="Settings" /> class from being created. You must instead use either
        ///     <see cref="Application" /> or <see cref="User" />
        /// </summary>
        /// <param name="type">The type.</param>
        private Settings(SettingType type)
        {
            _type = type;
        }

        /// <summary>
        ///     Initialises a <see cref="Settings" /> with a given provider.
        /// </summary>
        /// <param name="provider">The <see cref="ISettingsProvider" /> implementation to use.</param>
        public Settings(ISettingsProvider provider)
        {
            settingsProvider = provider;
        }

        /// <summary>
        ///     Accesses application settings
        /// </summary>
        public static Settings Application { get; } = new Settings(SettingType.Application);

        /// <summary>
        ///     Marks the end of an encrypted setting. Defaults to "[{ENC]{". NOTE: The brackets are
        ///     DELIBERATELY mis-matched, making the chance of a clash with real data ALMOST impossible!
        ///     <para>DO NOT USE ANY CLOSING CURLY BRACES IN THIS!</para>
        /// </summary>
        public static string EncryptionClosingIdentifier { get; set; } = "]{ENC[{";

        /// <summary>
        ///     Marks a string as being an encrypted setting if it starts with this. Defaults to "[{ENC]{". NOTE: The brackets are
        ///     DELIBERATELY mis-matched, making the chance of a clash with real data ALMOST impossible!
        ///     <para>DO NOT USE ANY CLOSING CURLY BRACES IN THIS!</para>
        /// </summary>
        public static string EncryptionOpeningIdentifier { get; set; } = "[{ENC]{";

        /// <summary>
        ///     Accesses user settings
        /// </summary>
        public static Settings User { get; } = new Settings(SettingType.User);

        /// <summary>
        ///     Gets or sets the <see cref="System.String" /> value with the specified key from the instance's
        ///     <seealso cref="Application" /> or
        ///     <seealso cref="User" /> settings
        ///     repository as appropriate. This is a convenient string
        ///     accessor, if you want to set or retrieve other types, you will need to use <see cref="Read{T,U}" /> or
        ///     <see cref="ReadEnum{T}" />.
        /// </summary>
        /// <value>
        ///     The <see cref="System.String" />.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns>
        ///     A string key.
        /// </returns>
        public string this[string key]
        {
            get
            {
                var value = string.Empty;
                Read(key, ref value);
                return value;
            }

            set => Write(key, value);
        }

        /// <summary>
        ///     The <see cref="ISettingsProvider" /> being used by the <see cref="Settings" />.
        /// </summary>
        public ISettingsProvider Provider
        {
            get
            {
                if (settingsProvider != null) return settingsProvider;

                return SettingsProvider;
            }
        }

        /// <summary>
        ///     Set this if you need to encrypt/decrypt settings. By default is set to <see cref="DpApiSettingsEncryptor" />
        /// </summary>
        public ISettingsEncryptor SettingsEncryptor { get; set; } =
            new DpApiSettingsEncryptor(DpApiSettingsEncryptor.ProtectionScope.Machine);

        /// <summary>
        ///     The root ISettingsProvider. Only the first provider to be assigned will be accepted. If you need to replace the
        ///     original provider you will first need to set
        ///     <see cref="SettingsProvider" /> to null, then set to the new value.
        /// </summary>
        /// <remarks>
        ///     If you set to null, then currently the provider on ALL threads will be nulled out. Basically this shouldn't
        ///     happen, but MIGHT in a testing environment where you're using mocked providers!
        ///     <seealso cref="ReplaceSettingsProvider" />
        /// </remarks>
        public ISettingsProvider SettingsProvider
        {
            get
            {
                if (_originalProvider == null)
                {
                    // No log provider has been set, so by default we use ourself.
                    _settingsProvider = new ThreadLocal<ISettingsProvider>();
#if DotNetCore
                    _originalProvider = new JsonConfigSettingsProvider();
#else
                    _originalProvider = new ConfigSettingsProvider();
#endif
                    _settingsProvider.Value = _originalProvider;
                }
                else if (_settingsProvider.Value == null)
                {
                    // There is no provider set up for THIS thread, so get the original one to clone itself!
                    _settingsProvider.Value = (ISettingsProvider) _originalProvider.Clone();
                }

                return _settingsProvider.Value;
            }

            set
            {
                if (value == null && _settingsProvider != null && _settingsProvider.Value != null)
                {
                    // We need to allow for the provider to be nulled out. Essentially only for use in unit tests
                    // where we might be expecting instantiated providers, generally mocked, to record certain behaviours.
                    _settingsProvider.Value = null;
                }
                else if (_settingsProvider == null && value != null)
                {
                    // Otherwise, only the first provider to get in determines the provider to use.
                    _settingsProvider = new ThreadLocal<ISettingsProvider>();
                    _originalProvider = value;
                    _settingsProvider.Value = value;
                }
                else if (_settingsProvider != null && (_settingsProvider.Value == null) & (value != null))
                {
                    // A specific provider is being set for this thread. This one WON'T be cloned, but the setter WILL be respected
                    _settingsProvider.Value = value;
                }
            }
        }

        /// <summary>
        ///     Decodes and then decrypts the supplied string
        /// </summary>
        /// <param name="readVal">The encoded, encrypted string</param>
        /// <param name="settingsEncryptor">The encryptor (well actually decryptor) to use</param>
        /// <returns>The decoded, decrypted string</returns>
        public static string Decrypt(string readVal, ISettingsEncryptor settingsEncryptor)
        {
            string retVal;

            if (settingsEncryptor == null) throw new SettingsException(SettingsExceptionReason.NoEncryptionProvider);

            // We now need to find each ocurrance of the Encryption Identifier and replace it's following string with the unencrypted version
            var moreEncryptedStrings = true;
            var start = 0;
            var stringBuilder = new StringBuilder();

            while (moreEncryptedStrings)
                try
                {
                    var preEncrypted = readVal.Extract(ref start, EncryptionOpeningIdentifier, false);

                    string base64Str;

                    try
                    {
                        base64Str = readVal.Extract(ref start, EncryptionClosingIdentifier, false);
                    }
                    catch (DataMisalignedException)
                    {
                        moreEncryptedStrings = false;
                        base64Str = readVal.Substring(start);
                    }

                    var encryptedData = Convert.FromBase64String(base64Str);
                    var decryptedStr = settingsEncryptor.Decrypt(encryptedData);
                    stringBuilder.Append(preEncrypted);
                    stringBuilder.Append(decryptedStr);
                }
                catch (DataMisalignedException)
                {
                    moreEncryptedStrings = false;
                    var lastSegment = readVal.Substring(start);
                    stringBuilder.Append(lastSegment);
                }
                catch (Exception excep)
                {
                    throw new SettingsException(excep, SettingsExceptionReason.UnknownException, nameof(Decrypt),
                        excep.Message);
                }

            retVal = stringBuilder.ToString();

            // If the decrypted string starts and ends with a " we need to actually strip them because
            // the original encryption encrypts the JSON (and for a string this INCLUDES quotes) but the read
            // IGNORES the quotes, but the encryption CONTAINS the quotes and these must be stripped off.
            // This doesn't happen with other types!
            if (retVal.StartsWith("\"") && retVal.EndsWith("\"")) retVal = retVal.Substring(1, retVal.Length - 2);

            return retVal;
        }

        /// <summary>
        ///     Flushes this instance.
        /// </summary>
        public void Flush()
        {
            Provider.Flush(_type);
        }

        /// <summary>
        ///     Gets the connection string.
        /// </summary>
        /// <param name="connectionStringKey">The connection string key.</param>
        /// <param name="connectionStringContainer">
        ///     The connection string container name in the settings file if you need to use a
        ///     different setting.
        /// </param>
        /// <returns>
        ///     The connection string, or null if it doesn't exist
        /// </returns>
        public string GetConnectionString(string connectionStringKey,
            string connectionStringContainer = "ConnectionStrings")
        {
            string retVal = null;

            try
            {
                var connectionStrings = new Dictionary<string, string>();
                Read(connectionStringContainer, ref connectionStrings);
                connectionStrings.TryGetValue(connectionStringKey, out retVal);
            }
            catch (Exception)
            {
            }

            return retVal;
        }

        /// <summary>
        ///     Reads the specified key into a supplied property.
        ///     <para>
        ///         Usage: Settings.Application.Read("LoggingFile", SettingsConfig, ()=>SettingsConfig.LogFilename)
        ///     </para>
        /// </summary>
        /// <remarks>
        ///     You can make good use of the <see cref="addIfNotExists" /> flag being true to create the structure of a
        ///     configuration object for you in the settings file. You'll do this towards the beginning of your project or anytime
        ///     you add a significant amount of new settings. Just note however that the settings file that the setting will be
        ///     written into, where it doesn't exist, will be the one in your - generally - bin\Debug folder. So you'll edit that,
        ///     grab the default settings and paste it into your ACTUAL settings file in source - remember it gets copied over at
        ///     build time!
        ///     <para>
        ///         Another thing to note is that if the setting that's being read exists it won't write any new sub-settings, so
        ///         if you add a significant new amount you want to be created for you, you'll want to comment out the main
        ///         setting, allow the settings to be created and then copy over and edit them in to your real settings.
        ///     </para>
        ///     <para>
        ///         It'll make sense to you as you use it!
        ///     </para>
        ///     <para>
        ///         And finally the writing out of the missing settings DOES NOT WRITE ENUMS, only as numbers so you'll want to
        ///         correct those values in your settings file where you have got enums to be the readable enum values!
        ///     </para>
        /// </remarks>
        /// <typeparam name="T">The type of the instance that has the property</typeparam>
        /// <typeparam name="U">The type of the property</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="instance">The instance.</param>
        /// <param name="property">The property.</param>
        /// <param name="addIfNotExists">Determines if the setting should be added if it doesn't exist.</param>
        /// <returns>True if the value to be read was found</returns>
        public bool Read<T, U>(string key, T instance, Expression<Func<U>> property, bool addIfNotExists = false)
        {
            var referableValue = property.Compile()();

            if (referableValue == null)
                throw new ArgumentException("The property is null, please assign it before passing by reference!",
                    nameof(property));

            var retVal = Read(key, ref referableValue, addIfNotExists);
            var expr = (MemberExpression) property.Body;
            var prop = (PropertyInfo) expr.Member;
            prop.SetValue(instance, referableValue, null);

            return retVal;
        }

        /// <summary>
        ///     Reads an application config element as the generic type
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">
        ///     in - the default value to use if key doesn't exist, and also the value to use if consequently to be
        ///     written, out - the value in config
        /// </param>
        /// <returns>True if the value to be read was found</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Cannot use this function with a + typeof(T).Name</exception>
        /// <exception cref="InvalidDataException">
        ///     Invalid [int] value for + key
        ///     or
        ///     Invalid [double] value for + key
        ///     or
        ///     Invalid [DateTime] value for + key
        ///     or
        ///     Invalid [Guid] value for + key
        /// </exception>
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1503:CurlyBracketsMustNotBeOmitted",
            Justification = "I excuse throws!")]
        public bool Read<T>(string key, out T value)
        {
            value = default;
            return Read(key, ref value);
        }

        /// <summary>
        ///     Reads an application config element as the generic type - and can add it if it doesn't exist!
        /// </summary>
        /// <remarks>
        ///     You can make good use of the <see cref="addIfNotExists" /> flag being true to create the structure of a
        ///     configuration object for you in the settings file. You'll do this towards the beginning of your project or anytime
        ///     you add a significant amount of new settings. Just note however that the settings file that the setting will be
        ///     written into, where it doesn't exist, will be the one in your - generally - bin\Debug folder. So you'll edit that,
        ///     grab the default settings and paste it into your ACTUAL settings file in source - remember it gets copied over at
        ///     build time!
        ///     <para>
        ///         Another thing to note is that if the setting that's being read exists it won't write any new sub-settings, so
        ///         if you add a significant new amount you want to be created for you, you'll want to comment out the main
        ///         setting, allow the settings to be created and then copy over and edit them in to your real settings.
        ///     </para>
        ///     <para>
        ///         It'll make sense to you as you use it!
        ///     </para>
        ///     <para>
        ///         And finally the writing out of the missing settings DOES NOT WRITE ENUMS, only as numbers so you'll want to
        ///         correct those values in your settings file where you have got enums to be the readable enum values!
        ///     </para>
        /// </remarks>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">
        ///     in - the default value to use if key doesn't exist, and also the value to use if consequently to be
        ///     written, out - the value in config
        /// </param>
        /// <param name="addIfNotExists">If true adds the key to the config if it doesn't already exist</param>
        /// <returns>True if the value to be read was found</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Cannot use this function with a + typeof(T).Name</exception>
        /// <exception cref="InvalidDataException">
        ///     Invalid [int] value for + key
        ///     or
        ///     Invalid [double] value for + key
        ///     or
        ///     Invalid [DateTime] value for + key
        ///     or
        ///     Invalid [Guid] value for + key
        /// </exception>
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1503:CurlyBracketsMustNotBeOmitted",
            Justification = "I excuse throws!")]
        public bool Read<T>(string key, ref T value, bool addIfNotExists = false)
        {
            var retVal = false;
            PerformAnyInitialRead();
            var setting = ReadCommon(key, _type);

            var valueType = typeof(T);

            var jsonIt = valueType != typeof(string) &&
                         valueType != typeof(int) &&
                         valueType != typeof(double) &&
                         valueType != typeof(bool) &&
                         valueType != typeof(DateTime) &&
                         valueType != typeof(DateTimeOffset) &&
                         valueType != typeof(Guid);

            if (setting == null)
            {
                if (addIfNotExists)
                {
                    var writeThis = "Couldn't create the string representation!";

                    writeThis = jsonIt ? JsonConvert.SerializeObject(value) : value.ToString();
                    Provider.WriteSetting(key, writeThis, value, _type);
                }
            }
            else
            {
                retVal = true;

                if (typeof(T) == typeof(string))
                {
                    value = (T) (object) setting;
                }
                else if (typeof(T) == typeof(int))
                {
                    if (!int.TryParse(setting, out var intVal))
                        throw new InvalidDataException($"Invalid {typeof(T).Name} value, [{setting}] for {key}");

                    value = (T) (object) intVal;
                }
                else if (typeof(T) == typeof(double))
                {
                    if (!double.TryParse(setting, out var doubleVal))
                        throw new InvalidDataException($"Invalid {typeof(T).Name} value, [{setting}] for {key}");

                    value = (T) (object) doubleVal;
                }
                else if (typeof(T) == typeof(bool))
                {
                    var trimmedSetting = (setting ?? string.Empty).Trim();
                    setting = trimmedSetting.ToLower();

                    // For booleans we can handle true/false, t/f, yes/no, y/n, 1/0
                    switch (setting)
                    {
                        case "true":
                        case "t":
                        case "yes":
                        case "y":
                        case "1":
                            value = (T) (object) true;
                            break;
                        case "false":
                        case "f":
                        case "no":
                        case "n":
                        case "0":
                            value = (T) (object) false;
                            break;
                        default:
                            throw new InvalidDataException($"Invalid {typeof(T).Name} value, [{setting}] for {key}");
                    }
                }
                else if (typeof(T) == typeof(DateTime))
                {
                    if (!DateTime.TryParse(setting, out var dateVal))
                        throw new InvalidDataException($"Invalid {typeof(T).Name} value, [{setting}] for {key}");

                    value = (T) (object) dateVal;
                }
                else if (typeof(T) == typeof(DateTimeOffset))
                {
                    if (!DateTimeOffset.TryParse(setting, out var dateVal))
                        throw new InvalidDataException($"Invalid {typeof(T).Name} value, [{setting}] for {key}");

                    value = (T) (object) dateVal;
                }
                else if (typeof(T) == typeof(Guid))
                {
                    if (!Guid.TryParse(setting, out var guidVal))
                        throw new InvalidDataException($"Invalid {typeof(T).Name} value, [{setting}] for {key}");

                    value = (T) (object) guidVal;
                }
                else
                {
                    // Try to de-serialise the JSON string
                    value = JsonConvert.DeserializeObject<T>(setting);
                }
            }

            return retVal;
        }

        /// <summary>
        ///     Reads the specified key into a supplied property as an enum.
        ///     Usage: Settings.Application.Read("LoggingFile", SettingsConfig, ()=>SettingsConfig.LogFilename)
        /// </summary>
        /// <typeparam name="T">The type of the instance that has the property</typeparam>
        /// <typeparam name="U">The type of the property</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="instance">The instance.</param>
        /// <param name="property">The property.</param>
        /// <param name="addIfNotExists">Determines if the setting should be added if it doesn't exist.</param>
        /// <param name="what">
        ///     If the value is to be added, <see cref="WhatAttributeOfEnum" /> indicates how that enum will be
        ///     written.
        /// </param>
        /// <returns>True if the value to be read was found</returns>
        public bool ReadEnum<T, U>(string key,
            T instance,
            Expression<Func<U>> property,
            bool addIfNotExists = false,
            WhatAttributeOfEnum what = WhatAttributeOfEnum.EnumValue)
            where U : IConvertible
        {
            var referableValue = property.Compile()();

            if (referableValue == null)
                throw new ArgumentException(
                    $"The {property.Name} property is null, please assign it before passing by reference!",
                    nameof(property));

            var retVal = ReadEnum(key, ref referableValue, addIfNotExists, what);
            var expr = (MemberExpression) property.Body;
            var prop = (PropertyInfo) expr.Member;
            prop.SetValue(instance, referableValue, null);

            return retVal;
        }

        /// <summary>
        ///     Reads a typed enum from the config file. The enum will be parsed from a string into the enum and can be represented
        ///     in many ways, as per <see cref="WhatAttributeOfEnum" />.
        /// </summary>
        /// <typeparam name="T">The enum type being read</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">
        ///     in - the default value to use if key doesn't exist, and also the value to use if consequently to be
        ///     written, out - the value in config
        /// </param>
        /// <param name="addIfnotExists">If true adds the key to the config if it doesn't already exist</param>
        /// <param name="what">
        ///     If the value is to be added, <see cref="WhatAttributeOfEnum" /> indicates how that enum will be
        ///     written.
        /// </param>
        /// <returns>True if the value to be read was found</returns>
        /// <exception cref="System.ArgumentException">If <typeparamref name="T" /> is not an Enum</exception>
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1503:CurlyBracketsMustNotBeOmitted",
            Justification = "I excuse throws!")]
        public bool ReadEnum<T>(string key,
            ref T value,
            bool addIfnotExists = false,
            WhatAttributeOfEnum what = WhatAttributeOfEnum.EnumValue)
            where T : IConvertible
        {
            var retVal = false;

            if (!typeof(T).GetTypeInfo().IsEnum) throw new ArgumentException(typeof(T).Name + " is not an enum!");

            PerformAnyInitialRead();
            var setting = ReadCommon(key, _type);

            if (setting == null)
            {
                if (addIfnotExists)
                    try
                    {
                        var enumAttributeString = string.Empty;

                        switch (what)
                        {
                            case WhatAttributeOfEnum.NumericValue:
                                enumAttributeString = ((Enum) (object) value).ToInt().ToString();
                                break;
                            case WhatAttributeOfEnum.EnumValue:
                                enumAttributeString = ((Enum) (object) value).ToString();
                                break;
                            case WhatAttributeOfEnum.Description:
                                enumAttributeString = ((Enum) (object) value).OriginalDescription();
                                break;
                            case WhatAttributeOfEnum.ModifiedDescription:
                                enumAttributeString = ((Enum) (object) value).Description();
                                break;
                        }

                        Provider.WriteSetting(key, enumAttributeString, value, _type);
                    }
                    catch (Exception)
                    {
                        // We'll log and swallow exceptions on updating the config
                        // Logger.Error(excep, "Error whilst trying to update the config file. ");
                    }
            }
            else
            {
                retVal = true;

                try
                {
                    value = EnumExtensions.FromString<T>(setting);
                }
                catch (Exception)
                {
                    // Swallow the exception and assume we couldn't get the setting, leaving value at its default.
                }
            }

            return retVal;
        }

        /// <summary>
        ///     Refreshes any caches.
        /// </summary>
        public void RefreshAnyCaches()
        {
            Provider.RefreshAnyCaches(_type);
        }

        /// <summary>
        ///     Replaces the settings provider - across the board, not just for the calling thread.
        /// </summary>
        /// <param name="newOne">The new one.</param>
        public void ReplaceSettingsProvider(ISettingsProvider newOne)
        {
            _settingsProvider = null;
            SettingsProvider = newOne;
        }

        /// <summary>
        ///     Writes the specified key and value to the indicated settings via the current <see cref="ISettingsProvider" />. DO
        ///     NOT USE THIS TO STORE SETTINGS IN THE APPLICATION SETTINGS AT RUNTIME> THIS IS VERY BAD PRACTICE! Fine for user
        ///     settings, well crucial actually, OR for creating application settings files away from runtime.
        /// </summary>
        /// <typeparam name="T">
        ///     The type of the value being written - This is "autofilled" by the compiler based on what type of
        ///     value you actually pass!
        /// </typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="encrypt">If true, the value should be written encrypted</param>
        public void Write<T>(string key, T value, bool encrypt = false)
        {
            var jsonIt = !typeof(T).GetTypeInfo().IsValueType && !(value is string);

            try
            {
                var writeThis = jsonIt ? JsonConvert.SerializeObject(value, Formatting.Indented) : value.ToString();

                if (encrypt)
                {
                    writeThis = Encrypt(writeThis);
                    Provider.WriteSetting(key, writeThis, writeThis, _type);
                }
                else
                {
                    Provider.WriteSetting(key, writeThis, value, _type);
                }
            }
            catch (SettingsException)
            {
                throw;
            }
            catch (Exception)
            {
                // We'll log and swallow any other exceptions on updating the config
            }
        }

        /// <summary>
        ///     Encrypts and then encodes the supplied string
        /// </summary>
        /// <param name="writeThis">The clear string</param>
        /// <returns>The encoded, encrypted string</returns>
        private string Encrypt(string writeThis)
        {
            string retVal;

            if (SettingsEncryptor == null) throw new SettingsException(SettingsExceptionReason.NoEncryptionProvider);

            var encryptedData = SettingsEncryptor.Encrypt(writeThis);
            var base64Str = Convert.ToBase64String(encryptedData);

            retVal = $"{EncryptionOpeningIdentifier}{base64Str}{EncryptionClosingIdentifier}";
            return retVal;
        }

        private void PerformAnyInitialRead()
        {
            if (!Provider.CacheRead)
            {
                Provider.CacheRead = true;
                RefreshAnyCaches();
            }
        }

        /// <summary>
        ///     Common setting read method. Decrypts any encrypted settings
        /// </summary>
        /// <param name="key">The key for the setting to read</param>
        /// <param name="type">The type of the setting</param>
        /// <returns>A setting ready for further processing</returns>
        private string ReadCommon(string key, SettingType type)
        {
            var readVal = Provider.ReadSetting(key, type);

            // We need to check if the string is an encrypted string
            if (readVal != null && readVal.Contains(EncryptionOpeningIdentifier))
                try
                {
                    readVal = Decrypt(readVal, SettingsEncryptor);
                }
                catch (CryptographicException excep)
                {
                    throw new SettingsException(excep, SettingsExceptionReason.InvalidData, key);
                }

            return readVal;
        }
    }
}