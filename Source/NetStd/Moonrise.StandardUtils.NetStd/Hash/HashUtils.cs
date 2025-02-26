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

using System.Security.Cryptography;
using System.Text;

namespace Moonrise.Utils.Standard.Hash
{
    /// <summary>
    ///     Utility methods for working with hashes.
    /// </summary>
    public class HashUtils
    {
        /// <summary>
        ///     Computes the MD5 hash value of a string and returns the hash as a 32-character, hexadecimal-formatted string
        /// </summary>
        /// <param name="md5Hash">The <see cref="MD5" /> instance to be used to generate the hash</param>
        /// <param name="input">The string to hash.</param>
        /// <returns>The hex-formatted hash string.</returns>
        public static string GetMd5Hash(MD5 md5Hash, string input)
        {
            // Convert the input string to a byte array and compute the hash. 
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes 
            // and create a string.
            StringBuilder hashResult = new StringBuilder();

            // Loop through each byte of the hashed data  
            // and format each one as a hexadecimal string. 
            for (int i = 0; i < data.Length; i++)
            {
                hashResult.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string. 
            return hashResult.ToString();
        }
    }
}