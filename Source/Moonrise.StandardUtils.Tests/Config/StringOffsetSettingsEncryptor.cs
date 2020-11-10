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
using System.Text;
using Moonrise.Utils.Standard.Config;

namespace Moonrise.StandardUtils.Tests.Config
{
    public class StringOffsetSettingsEncryptor : ISettingsEncryptor
    {
        public string Decrypt(byte[] encryptedSettings)
        {
            string retVal = Encoding.Unicode.GetString(encryptedSettings);
            StringBuilder builder = new StringBuilder();

            foreach (char character in retVal)
            {
                builder.Append((char)(Convert.ToUInt16(character) - 1));
            }

            retVal = builder.ToString();
            return retVal;
        }

        public byte[] Encrypt(string unencryptedSetting)
        {
            byte[] retVal;

            StringBuilder builder = new StringBuilder();

            foreach (char character in unencryptedSetting)
            {
                builder.Append((char)(Convert.ToUInt16(character) + 1));
            }

            retVal = Encoding.Unicode.GetBytes(builder.ToString());
            return retVal;
        }
    }
}
