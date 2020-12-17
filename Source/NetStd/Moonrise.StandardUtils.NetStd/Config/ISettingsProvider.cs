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

namespace Moonrise.Utils.Standard.Config
{
    /// <summary>
    ///     Indicates the type of setting to read or write
    /// </summary>
    public enum SettingType
    {
        /// <summary>
        ///     The application setting is shared amongst users
        /// </summary>
        Application,

        /// <summary>
        ///     The user setting is specific to a user
        /// </summary>
        User
    }

    /// <summary>
    ///     Provides read/write access to user and application settings
    /// </summary>
    public interface ISettingsProvider : ICloneable
    {
        /// <summary>
        ///     Only used by the <see cref="Settings" /> class, but indicates if this provider has read its cache yet.
        /// </summary>
        bool CacheRead { get; set; }

        /// <summary>
        /// Flushes the setting file, writing it out
        /// </summary>
        /// <param name="type">The type of the setting file to flush</param>
        void Flush(SettingType type);

        /// <summary>
        ///     Reads an application or user setting.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="type">The type of setting.</param>
        /// <returns>
        ///     The value, or null if not present
        /// </returns>
        string ReadSetting(string key, SettingType type);

        /// <summary>
        ///     Refreshes any caches that might be in use. i.e. Re-read.
        /// </summary>
        /// <param name="type">The type of setting.</param>
        void RefreshAnyCaches(SettingType type);

        /// <summary>
        ///     Writes the application setting.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value as a string - Use the most appropriate.</param>
        /// <param name="objval">The value as an object - Use the most appropriate.</param>
        /// <param name="type">The type of setting.</param>
        void WriteSetting(string key, string value, object objval, SettingType type);

        /// <summary>
        /// Reads the complete settings file as a single string
        /// </summary>
        /// <param name="type">The type of setting.</param>
        /// <returns>Wot I said</returns>
        string ReadCompleteFile(SettingType settingType);

        /// <summary>
        /// Writes the complete settings file as a single string
        /// </summary>
        /// <param name="settings">The complete settings</param>
        /// <param name="type">The type of setting.</param>
        void WriteCompleteFile(string settings, SettingType settingType);
    }
}
