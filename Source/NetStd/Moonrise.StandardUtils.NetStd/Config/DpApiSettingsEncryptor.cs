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
using System.Security.Cryptography;
using System.Text;

namespace Moonrise.Utils.Standard.Config
{
    /// <summary>
    ///     A settings encryptor that uses the Windows Data Protection API to encrypt settings.
    /// </summary>
    /// <seealso cref="Moonrise.Utils.Standard.Config.ISettingsEncryptor" />
    public class DpApiSettingsEncryptor : ISettingsEncryptor
    {
        private readonly DataProtectionScope scope;

        /// <summary>
        ///     Creates an instance of the <see cref="DpApiSettingsEncryptor" /> with either machine or user level scope.
        /// </summary>
        /// <param name="_scope">The level of scope for protection</param>
        public DpApiSettingsEncryptor(ProtectionScope _scope)
        {
            scope = (DataProtectionScope)_scope;
        }

        /// <summary>
        /// Indicates what data protection scope to use
        /// </summary>
        public enum ProtectionScope
        {
            /// <summary>
            /// Encrypts using the current user data protection scope. The current user can decrypt on any machine
            /// </summary>
            User = DataProtectionScope.CurrentUser,

            /// <summary>
            /// Encrypts using the current machine data protection scope. Any user on the machine can decrypt but can only do so on the same machine that encrypted
            /// </summary>
            Machine = DataProtectionScope.LocalMachine
        }

        /// <summary>
        ///     Decrypts the specified string.
        /// </summary>
        /// <param name="encryptedSettings">The encrypted setting.</param>
        /// <param name="additionalEntropy">Additional entropy required to decrypt. Defaults to null</param>
        /// <returns>
        ///     The decrypted setting
        /// </returns>
        public string Decrypt(byte[] encryptedSettings, string additionalEntropy = null)
        {
            byte[] decrypted = ProtectedData.Unprotect(encryptedSettings, additionalEntropy != null ? Encoding.ASCII.GetBytes(additionalEntropy) : null, scope);
            string retVal = Encoding.Unicode.GetString(decrypted);

            return retVal;
        }

        /// <summary>
        ///     Encrypts the specified string.
        /// </summary>
        /// <param name="unencryptedSetting">The unencrypted setting.</param>
        /// <param name="additionalEntropy">Additional entropy required to encrypt. Defaults to null</param>
        /// <returns>
        ///     The encrypted setting as
        /// </returns>
        public byte[] Encrypt(string unencryptedSetting, string additionalEntropy = null)
        {
            byte[] retVal = ProtectedData.Protect(Encoding.Unicode.GetBytes(unencryptedSetting), additionalEntropy != null ?Encoding.ASCII.GetBytes(additionalEntropy):null, scope);
            return retVal;
        }
    }
}
