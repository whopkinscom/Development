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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Moonrise.Utils.Standard.Extensions;
using Moonrise.Utils.Standard.Files;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Moonrise.Utils.Standard.Config
{
    /// <summary>
    ///     Provides access to settings stored in a JSON file. This is NOT restricted to just using within using Dot Net Core.
    ///     However, the Read and Writes will only read or write settings at the topmost level once inside the application
    ///     settings section.
    /// </summary>
    public class JsonConfigSettingsProvider : ISettingsProvider
    {
        /// <summary>
        ///     The configuration required
        /// </summary>
        public class Config
        {
            /// <summary>
            ///     The name of the settings filename, defaults to the DotNotCore default settings filename
            /// </summary>
            public string ApplicationSettingsFilename { get; set; } = "appSettings.json";

            /// <summary>
            ///     The folder the settings file is in, use null (the default) for the same folder as the application
            /// </summary>
            public string ApplicationSettingsFolder { get; set; }

            /// <summary>
            ///     Specifies a section in the config file where the application settings will be taken from, defaults to that as used
            ///     by DotNetCore but you don't have to have one if you're not sharing the settings with DotNetCore infrastructure
            ///     settings
            /// </summary>
            public string ApplicationSettingsSection { get; set; } = "";

            /// <summary>
            ///     The environment variable, if used, that contains the name of the settings override post-fix, defaults to that used
            ///     by DotNetCore
            /// </summary>
            public string ConfigurationOverrideEnvVar { get; set; } = "ASPNETCORE_ENVIRONMENT";

            /// <summary>
            ///     The name of the user settings file - this will be stored in the roaming or normal user profile location.
            /// </summary>
            public string UserSettingsFilename { get; set; } = "UserSettings.json";
        }

        /// <summary>
        ///     Defines encryption/decryption that will be applied to sections before writing and after reading.
        /// </summary>
        public interface ISettingsEncryptor
        {
            /// <summary>
            ///     Decrypts the specified jsoned data.
            /// </summary>
            /// <param name="jsonedData">The jsoned data.</param>
            /// <returns>The decrypted data</returns>
            string Decrypt(string jsonedData);

            /// <summary>
            ///     Encrypts the specified string.
            /// </summary>
            /// <param name="jsonedData">The jsoned dictionary.</param>
            /// <returns>The encrypted string</returns>
            string Encrypt(string jsonedData);
        }

        /// <summary>
        ///     The configuration override setting
        /// </summary>
        private readonly string ConfigurationOverride;

        /// <summary>
        ///     The configuration override environment variable - Kept purely for the Clone.
        /// </summary>
        private readonly string ConfigurationOverrideEnvVar;

        /// <summary>
        ///     Cached application settings
        /// </summary>
        private Dictionary<string, object> _applicationSettings = new Dictionary<string, object>();

        /// <summary>
        ///     Cached user settings
        /// </summary>
        private Dictionary<string, object> _userSettings = new Dictionary<string, object>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="JsonConfigSettingsProvider" /> class.
        /// </summary>
        /// <param name="applicationSettingsFilename">The application settings filename.</param>
        /// <param name="applicationSettingsFolder">
        ///     The application settings folder. If null (default) then the
        ///     <see cref="FileUtils.ApplicationPath" /> is used.
        /// </param>
        /// <param name="applicationSettingsSection">The application settings section name.</param>
        /// <param name="userSettingsFilename">The user settings filename.</param>
        /// <param name="configurationOverrideEnvVar">
        ///     The configuration override environment variable - Indicates the name of the
        ///     env var that holds the deployed environment allowing for configuration overrides.
        /// </param>
        [Obsolete("Please use the constructor that takes the Config class. This one WILL get removed")]
        public JsonConfigSettingsProvider(
            string applicationSettingsFilename,
            string applicationSettingsFolder,
            string applicationSettingsSection = "",
            string userSettingsFilename = "UserSettings.json",
            string configurationOverrideEnvVar = "ASPNETCORE_ENVIRONMENT") : this(new Config
                                                                                  {
                                                                                      ApplicationSettingsFilename =
                                                                                          applicationSettingsFilename,
                                                                                      ApplicationSettingsFolder =
                                                                                          applicationSettingsFolder,
                                                                                      ApplicationSettingsSection =
                                                                                          applicationSettingsSection,
                                                                                      ConfigurationOverrideEnvVar =
                                                                                          configurationOverrideEnvVar,
                                                                                      UserSettingsFilename = userSettingsFilename
                                                                                  }) { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="JsonConfigSettingsProvider" /> class.
        /// </summary>
        /// <param name="configuration">The configuration to use</param>
        public JsonConfigSettingsProvider(Config configuration = null)
        {
            if (configuration == null)
            {
                Configuration = new Config();
            }
            else
            {
                Configuration = configuration;
            }

            UserSettingsFilename = Configuration.UserSettingsFilename;
            ConfigurationOverrideEnvVar = Configuration.ConfigurationOverrideEnvVar;

            if (!string.IsNullOrWhiteSpace(ConfigurationOverrideEnvVar))
            {
                ConfigurationOverride = Environment.GetEnvironmentVariable(ConfigurationOverrideEnvVar);
            }

            if (Configuration.ApplicationSettingsFolder == null)
            {
                Configuration.ApplicationSettingsFolder = FileUtils.ApplicationPath();
            }

            ApplicationSettingsFilename = Path.Combine(Configuration.ApplicationSettingsFolder, Configuration.ApplicationSettingsFilename);
            ApplicationSettingsSection = Configuration.ApplicationSettingsSection;
        }

        /// <summary>
        ///     The application settings filename.
        /// </summary>
        public string ApplicationSettingsFilename { get; set; }

        /// <summary>
        ///     The application settings section name.
        /// </summary>
        public string ApplicationSettingsSection { get; set; }

        /// <summary>
        ///     Only used by the <see cref="T:Moonrise.Utils.Standard.Config.Settings" /> class, but indicates if this provider has
        ///     read its cache yet.
        /// </summary>
        public bool CacheRead { get; set; }

        /// <summary>
        ///     The configuration for this provider
        /// </summary>
        public Config Configuration { get; set; }

        /// <summary>
        ///     Set this if you need to encrypt/decrypt settings.
        /// </summary>
        public ISettingsEncryptor SettingsEncryptor { get; set; }

        /// <summary>
        ///     The user settings filename.
        /// </summary>
        public string UserSettingsFilename { get; set; }

        /// <summary>
        ///     Clones this instance.
        /// </summary>
        /// <returns>A new <see cref="JsonConfigSettingsProvider" /> for use by another thread</returns>
        public object Clone()
        {
            JsonConfigSettingsProvider retVal = new JsonConfigSettingsProvider(Configuration);
            retVal._applicationSettings = _applicationSettings;
            retVal._userSettings = _userSettings;
            return retVal;
        }

        /// <summary>
        ///     Write out the current settings
        /// </summary>
        /// <param name="type">The type of setting.</param>
        public void Flush(SettingType type)
        {
            if (type == SettingType.Application)
            {
                WriteApplicationSettingFile();
            }
            else
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
                return ReadApplicationSetting(key);
            }

            if (type == SettingType.User)
            {
                return ReadUserSetting(key);
            }

            return null;
        }

        /// <summary>
        ///     Refreshes any caches.
        /// </summary>
        public void RefreshAnyCaches(SettingType type)
        {
            if (type == SettingType.Application)
            {
                ReadApplicationSettingsFile();
            }
            else
            {
                ReadUserSettingsFile();
            }
        }

        /// <summary>
        ///     Writes the application setting.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value as a string - Use the most appropriate.</param>
        /// <param name="objval">The value as an object - Use the most appropriate.</param>
        /// <param name="type">The type of setting.</param>
        public void WriteSetting(string key,
                                 string value,
                                 object objval,
                                 SettingType type = SettingType.Application)
        {
            if (type == SettingType.Application)
            {
                WriteApplicationSetting(key, objval);
            }
            else
            {
                WriteUserSetting(key, objval);
            }
        }

        /// <summary>
        ///     Reads the application setting.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The value</returns>
        private string ReadApplicationSetting(string key)
        {
            string retVal = null;
            object setting;

            if (key.Contains(":"))
            {
                try
                {
                    // We've got a composite key
                    string[] keys = key.Split(':');
                    setting = _applicationSettings;

                    int level = 0;

                    foreach (string subkey in keys)
                    {
                        if (level++ == 0)
                        {
                            setting = ((IDictionary<string, object>)setting)[subkey];
                        }
                        else
                        {
                            if (subkey.Contains("["))
                            {
                                int start = 0;
                                string indexStr = subkey.Extract(ref start, "[", "]");
                                string subKey = subkey.Substring(0, subkey.IndexOf("[", StringComparison.CurrentCulture));
                                int index = int.Parse(indexStr);
                                setting = ((JObject)setting)[subKey];
                                setting = ((JArray)setting)[index];
                            }
                            else
                            {
                                setting = ((JObject)setting)[subkey];
                            }
                        }
                    }

                    if (setting is JValue && ((JValue)setting).Value is bool)
                    {
                        retVal = ((JValue)setting).Value.ToString().ToLower();
                    }
                    else
                    {
                        retVal = setting.ToString();
                    }
                }
                catch (KeyNotFoundException)
                {
                    return retVal;
                }
                catch (Exception e)
                {
                    throw new SettingsException(e, SettingsExceptionReason.InvalidKey, key);
                }
            }
            else if (_applicationSettings.TryGetValue(key, out setting))
            {
                retVal = setting.ToString();
            }

            return retVal;
        }

        /// <summary>
        ///     Reads the application setting file from the application folder
        /// </summary>
        private void ReadApplicationSettingsFile()
        {
            _applicationSettings.Clear();
            _applicationSettings = GetDictionary(ApplicationSettingsFilename);

            // If there is an override specified in the environment, read any additional settings specified there.
            if (!string.IsNullOrWhiteSpace(ConfigurationOverride))
            {
                string overrideSettingsFilename =
                    $"{Path.GetDirectoryName(ApplicationSettingsFilename)}\\{Path.GetFileNameWithoutExtension(ApplicationSettingsFilename)}.{ConfigurationOverride}{Path.GetExtension(ApplicationSettingsFilename)}";
                Dictionary<string, object> overrideDictionary = GetDictionary(overrideSettingsFilename);

                // Add any additional or replace any existing keys from the main settings file
                overrideDictionary.ToList().ForEach(ReplaceEachHighLevelSettings);
            }

            // Get the dictionary from the JSON settings file - original or override, it's the same
            Dictionary<string, object> GetDictionary(string settingsFilename)
            {
                // The default is an empty dictionary, rather than a null
                Dictionary<string, object> retVal = new Dictionary<string, object>();

                string jsonedData = FileUtils.ReadFile(settingsFilename);

                if (!string.IsNullOrEmpty(jsonedData))
                {
                    if (!string.IsNullOrWhiteSpace(ApplicationSettingsSection))
                    {
                        JObject wholeJson = JObject.Parse(jsonedData);

                        if (SettingsEncryptor != null)
                        {
                            jsonedData = wholeJson[ApplicationSettingsSection].ToString();
                            jsonedData = SettingsEncryptor.Decrypt(jsonedData);
                            wholeJson[ApplicationSettingsSection] = jsonedData;
                        }

                        // OK, this is a combined application settings and other settings JSON file!
                        JToken appSettings = wholeJson[ApplicationSettingsSection];
                        jsonedData = appSettings.ToString();
                    }
                    else if (SettingsEncryptor != null)
                    {
                        jsonedData = SettingsEncryptor.Decrypt(jsonedData);
                    }

                    retVal = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonedData);
                }

                return retVal;
            }

            // Nested "internal" method to allow iterative replacement of each overidden highest level setting
            void ReplaceEachHighLevelSettings(KeyValuePair<string, object> keyValuePair)
            {
                if (!_applicationSettings.ContainsKey(keyValuePair.Key))
                {
                    // Add the high level setting if it doesn't exist
                    _applicationSettings[keyValuePair.Key] = keyValuePair.Value;
                }
                else
                {
                    // Is the value a JSON object
                    if (keyValuePair.Value.GetType().IsAssignableFrom(typeof(JObject)))
                    {
                        // Look to iterate through the various JObject structures to replace/add anything else
                        JObject original = (JObject)_applicationSettings[keyValuePair.Key];
                        JObject replacement = (JObject)keyValuePair.Value;

                        ReplaceOveriddenSetting(original, replacement);
                    }
                    else
                    {
                        // OK, it's a straightforward type so we can simply assign
                        _applicationSettings[keyValuePair.Key] = keyValuePair.Value;
                    }
                }

                // Nested "internal" method to allow replacement of each overidden JSON setting
                void ReplaceOveriddenSetting(JObject original, JObject replacement)
                {
                    if (!original.HasValues)
                    {
                        if (replacement.HasValues)
                        {
                            original.Add(replacement.Children());
                        }
                    }
                    else
                    {
                        ReplaceEachChild(original, replacement);
                    }

                    // Nested "internal" method to allow recursive replacement of each overidden JSON setting
                    void ReplaceEachChild(JToken originalChild, JToken replacementChild)
                    {
                        if (replacementChild.Type == JTokenType.Object)
                        {
                            foreach (JProperty replacingChild in replacementChild.Children<JProperty>())
                            {
                                string replacementPath = replacingChild.Name;

                                JToken originalItem = originalChild.SelectToken(replacementPath);

                                if (originalItem == null)
                                {
                                    ((JContainer)originalChild).Add(replacingChild);
                                }
                                else
                                {
                                    ReplaceEachChild(originalItem.Parent, replacingChild);
                                }
                            }
                        }
                        else if (replacementChild.Type == JTokenType.Array)
                        {
                            foreach (JToken replacingChild in replacementChild.Children())
                            {
                                string replacementPath = replacingChild.Path;

                                JToken originalItem = originalChild.SelectToken(replacementPath);

                                if (originalItem == null)
                                {
                                    ((JContainer)originalChild).Add(replacingChild);
                                }
                                else
                                {
                                    ReplaceEachChild(originalItem.Parent, replacingChild);
                                }
                            }
                        }
                        else if (replacementChild.Type == JTokenType.Property)
                        {
                            if (replacementChild.HasValues)
                            {
                                foreach (JToken replacingValue in replacementChild.Children())
                                {
                                    JToken originalValue = ((JProperty)originalChild).Value;

                                    if (replacingValue.HasValues)
                                    {
                                        ReplaceEachChild(originalValue, replacingValue);
                                    }
                                    else
                                    {
                                        ((JProperty)originalChild).Value = replacingValue;
                                    }
                                }
                            }
                            else
                            {
                                JProperty parent = (JProperty)originalChild.Parent;
                                parent.Value = replacementChild;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Reads the user setting.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The value</returns>
        private string ReadUserSetting(string key)
        {
            string retVal = null;
            object setting;

            if (key.Contains(":"))
            {
                // We've got a composite key
                string[] keys = key.Split(':');
                setting = _userSettings;

                int level = 0;

                foreach (string subkey in keys)
                {
                    if (level++ == 0)
                    {
                        setting = ((IDictionary<string, object>)setting)[subkey];
                    }
                    else
                    {
                        if (subkey.Contains("["))
                        {
                            int start = 0;
                            string indexStr = subkey.Extract(ref start, "[", "]");
                            string subKey = subkey.Substring(0, subkey.IndexOf("[", StringComparison.CurrentCulture));
                            int index = int.Parse(indexStr);
                            setting = ((JObject)setting)[subKey];
                            setting = ((JArray)setting)[index];
                        }
                        else
                        {
                            setting = ((JObject)setting)[subkey];
                        }
                    }
                }

                retVal = setting.ToString();
            }
            else if (_userSettings.TryGetValue(key, out setting))
            {
                retVal = setting.ToString();
            }

            return retVal;
        }

        /// <summary>
        ///     Reads the user setting file from the roaming user path
        /// </summary>
        private void ReadUserSettingsFile()
        {
            _userSettings.Clear();

            if (!string.IsNullOrWhiteSpace(UserSettingsFilename))
            {
                string jsonedDict = FileUtils.ReadFile(FileUtils.RoamingUserApplicationPath(), UserSettingsFilename);

                if (!string.IsNullOrEmpty(jsonedDict))
                {
                    if (SettingsEncryptor != null)
                    {
                        jsonedDict = SettingsEncryptor.Decrypt(jsonedDict);
                    }

                    _userSettings = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonedDict);
                }
            }
        }

        /// <summary>
        ///     Writes the application setting into the application config file.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value as an object</param>
        private void WriteApplicationSetting(string key, object value)
        {
            if (key.Contains(":"))
            {
                // We've got a composite key
                string[] keys = key.Split(':');
                object setting = _applicationSettings;

                int level = 0;

                foreach (string subkey in keys)
                {
                    try
                    {
                        if (level++ == 0)
                        {
                            if (level == keys.Length)
                            {
                                ((IDictionary<string, object>)setting)[subkey] = value;
                            }
                            else
                            {
                                setting = ((IDictionary<string, object>)setting)[subkey];
                            }
                        }
                        else
                        {
                            if (level == keys.Length)
                            {
                                if (subkey.Contains("["))
                                {
                                    int start = 0;
                                    string indexStr = subkey.Extract(ref start, "[", "]");
                                    string subKey = subkey.Substring(0, subkey.IndexOf("[", StringComparison.CurrentCulture));
                                    int index = int.Parse(indexStr);
                                    setting = ((JObject)setting)[subKey];
                                    setting = ((JArray)setting)[index];
                                    ((JValue)setting).Value = value;
                                }
                                else
                                {
                                    ((JObject)setting)[subkey] = JToken.FromObject(value);
                                }
                            }
                            else
                            {
                                if (subkey.Contains("["))
                                {
                                    int start = 0;
                                    string indexStr = subkey.Extract(ref start, "[", "]");
                                    string subKey = subkey.Substring(0, subkey.IndexOf("[", StringComparison.CurrentCulture));
                                    int index = int.Parse(indexStr);
                                    setting = ((JObject)setting)[subKey];
                                    setting = ((JArray)setting)[index];
                                }
                                else
                                {
                                    setting = ((JObject)setting)[subkey];
                                }
                            }
                        }
                    }
                    catch (KeyNotFoundException excep)
                    {
                        throw new SettingsException(excep, SettingsExceptionReason.InvalidKey, key);
                    }
                }
            }
            else
            {
                _applicationSettings[key] = value;
            }

            WriteApplicationSettingFile();
        }

        /// <summary>
        ///     Writes the application setting file from the dictionary cache. Any override in place WILL BE IGNORED. This is not
        ///     meant for use at runtime!
        /// </summary>
        private void WriteApplicationSettingFile()
        {
            if (_applicationSettings.Count > 0)
            {
                string applicationJson = JsonConvert.SerializeObject(_applicationSettings, Formatting.Indented);

                // The settings we're dealing with MAY be part of a larger settings file, so make sure we preserve that fer gawdsake!
                if (!string.IsNullOrWhiteSpace(ApplicationSettingsSection))
                {
                    JObject appSettings = JObject.Parse(applicationJson);

                    JObject existingAppSettings = appSettings;
                    string existingJsonedData = FileUtils.ReadFile(ApplicationSettingsFilename);

                    if (!string.IsNullOrEmpty(existingJsonedData))
                    {
                        existingAppSettings = JObject.Parse(existingJsonedData);
                    }
                    else
                    {
                        existingAppSettings = new JObject();
                    }

                    if (SettingsEncryptor != null)
                    {
                        applicationJson = SettingsEncryptor.Encrypt(applicationJson);
                        existingAppSettings[ApplicationSettingsSection] = applicationJson;
                    }
                    else
                    {
                        existingAppSettings[ApplicationSettingsSection] = appSettings;
                    }

                    // If we're sitting inside a section of a bigger JSON file, then we have to treat the appsettings, encrypted or otherwise
                    // as part of that overall JSON so we'll use the JSON writer to write out.
                    using (StreamWriter settingsFile =
                        File.CreateText(ApplicationSettingsFilename))
                    {
                        using (JsonTextWriter writer = new JsonTextWriter(settingsFile))
                        {
                            writer.Formatting = Formatting.Indented;
                            existingAppSettings.WriteTo(writer);
                        }
                    }
                }
                else
                {
                    // If thereis no application section we can just write out the JSON - encrypted or otherwise, as a string to a file.
                    if (SettingsEncryptor != null)
                    {
                        applicationJson = SettingsEncryptor.Encrypt(applicationJson);
                    }

                    File.WriteAllText(ApplicationSettingsFilename, applicationJson);
                }
            }
        }

        /// <summary>
        ///     Writes the user setting into the local dictionary and then updates the entire dictionary in the settings file in
        ///     the roaming user path
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        private void WriteUserSetting(string key, object value)
        {
            if (key.Contains(":"))
            {
                // We've got a composite key
                string[] keys = key.Split(':');
                object setting = _userSettings;

                int level = 0;

                foreach (string subkey in keys)
                {
                    if (level++ == 0)
                    {
                        if (level == keys.Length)
                        {
                            ((IDictionary<string, object>)setting)[subkey] = value;
                        }
                        else
                        {
                            setting = ((IDictionary<string, object>)setting)[subkey];
                        }
                    }
                    else
                    {
                        if (level == keys.Length)
                        {
                            if (subkey.Contains("["))
                            {
                                int start = 0;
                                string indexStr = subkey.Extract(ref start, "[", "]");
                                string subKey = subkey.Substring(0, subkey.IndexOf("[", StringComparison.CurrentCulture));
                                int index = int.Parse(indexStr);
                                setting = ((JObject)setting)[subKey];
                                setting = ((JArray)setting)[index];
                            }
                            else
                            {
                                ((JObject)setting)[subkey] = new JObject(value);
                            }
                        }
                        else
                        {
                            if (subkey.Contains("["))
                            {
                                int start = 0;
                                string indexStr = subkey.Extract(ref start, "[", "]");
                                string subKey = subkey.Substring(0, subkey.IndexOf("[", StringComparison.CurrentCulture));
                                int index = int.Parse(indexStr);
                                setting = ((JObject)setting)[subKey];
                                setting = ((JArray)setting)[index];
                            }
                            else
                            {
                                setting = ((JObject)setting)[subkey];
                            }
                        }
                    }
                }
            }
            else
            {
                _userSettings[key] = value;
            }

            WriteUserSettingFile();
        }

        /// <summary>
        ///     Writes the user setting file into the roaming user path
        /// </summary>
        private void WriteUserSettingFile()
        {
            if (_userSettings.Count > 1)
            {
                string jsonedDict = JsonConvert.SerializeObject(_userSettings);

                if (SettingsEncryptor != null)
                {
                    jsonedDict = SettingsEncryptor.Encrypt(jsonedDict);
                }

                FileUtils.WriteFile(FileUtils.RoamingUserApplicationPath(),
                                    UserSettingsFilename,
                                    jsonedDict);
            }
        }
    }

    /// <summary>
    ///     Initial reference implementation
    /// </summary>
    /// <seealso cref="Moonrise.Utils.Standard.Config.JsonConfigSettingsProvider.ISettingsEncryptor" />
    public class SampleEncryptor : JsonConfigSettingsProvider.ISettingsEncryptor
    {
        /// <summary>
        ///     Decrypts the specified jsoned data.
        /// </summary>
        /// <param name="jsonedData">The jsoned data.</param>
        /// <returns>The decrypted data</returns>
        public string Decrypt(string jsonedData)
        {
            string retVal;
            StringBuilder builder = new StringBuilder();

            foreach (char character in jsonedData)
            {
                builder.Append((char)(Convert.ToUInt16(character) - 1));
            }

            retVal = builder.ToString();
            return retVal;
        }

        /// <summary>
        ///     Encrypts the specified string.
        /// </summary>
        /// <param name="jsonedData">The jsoned dictionary.</param>
        /// <returns>The encrypted string</returns>
        public string Encrypt(string jsonedData)
        {
            string retVal;
            StringBuilder builder = new StringBuilder();

            foreach (char character in jsonedData)
            {
                builder.Append((char)(Convert.ToUInt16(character) + 1));
            }

            retVal = builder.ToString();
            return retVal;
        }
    }
}
