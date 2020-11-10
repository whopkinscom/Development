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
using Moonrise.Utils.Standard.Config;

namespace Moonrise.StandardUtils.Tests.Config
{
    public class StringSettingsProvider : ISettingsProvider
    {
        public readonly Dictionary<string, string> settingsDictionary = new Dictionary<string, string>();

        public bool CacheRead { get; set; }

        public void Add(string key, string value)
        {
            settingsDictionary.Add(key, value);
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }

        public void Flush(SettingType type) { }

        public string ReadSetting(string key, SettingType type = SettingType.Application)
        {
            string retVal = string.Empty;

            if (settingsDictionary.ContainsKey(key))
            {
                retVal = settingsDictionary[key];
            }

            return retVal;
        }

        public void RefreshAnyCaches(SettingType type) { }

        public void WriteSetting(string key, string value, object objval, SettingType type = SettingType.Application)
        {
            Add(key, value);
        }
    }
}
