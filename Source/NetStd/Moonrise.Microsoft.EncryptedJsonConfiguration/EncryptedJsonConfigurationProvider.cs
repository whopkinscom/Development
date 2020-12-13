using System.IO;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Moonrise.Utils.Standard.Config;
using Moonrise.Utils.Standard.Files;
using Moonrise.Utils.Standard.Threading;

namespace Moonrise.Microsoft.Extensions.Configuration.EncryptedJsonConfiguration
{
    /// <summary>
    ///     A potentially encrypted JSON based <see cref="FileConfigurationProvider" />.
    /// </summary>
    public class EncryptedJsonConfigurationProvider : JsonConfigurationProvider
    {
        /// <summary>
        /// If you have re-encrypted a settings file via <see cref="Settings.ReEncrypt()"/> to make its encryption unique to a particular application and you want
        /// that application to also be able to use the <see cref="IConfiguration"/> access, then you must use <see cref="AdditionalEntropy"/> to pass/smuggle that
        /// value down to enable it to decrypt. You must do this prior to every call against the <see cref="IConfiguration"/>. As EACH individual read will reset the
        /// additional entropy value to null.
        /// </summary>
        /// <remarks>
        /// Do so as follows;
        /// <code>
        /// using (new EncryptedJsonConfigurationProvider.AdditionalEntropy(value you're using))
        /// {<para>
        /// ...</para><para>
        /// _configuration.Read....</para><para>
        /// }</para>
        /// </code>
        /// </remarks>
        public class AdditionalEntropy : ScopedNestableThreadGlobalSingleton<string>
        {
            public AdditionalEntropy(string additionalEntropy) : base(additionalEntropy)
            {
            }

            /// <summary>
            /// Resets the current threaded value to null.
            /// </summary>
            internal static void Reset()
            {
                Current = null;
            }
        }

        /// <summary>
        ///     Initialises a <see cref="EncryptedJsonConfigurationProvider" />.
        /// </summary>
        /// <param name="source">The source for the provider</param>
        public EncryptedJsonConfigurationProvider(JsonConfigurationSource source) : base(source)
        {
        }

        /// <summary>
        ///     Loads the JSON data from a stream, decoding any encrypted settings before handing it on to its base implementation
        /// </summary>
        /// <param name="stream">The stream of JSON data that MIGHT contain encoded encrypted settings.</param>
        public override void Load(Stream stream)
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                string jsonContent = reader.ReadToEnd();

                if (jsonContent.Contains(Settings.EncryptionOpeningIdentifier))
                {
                    // First we need to strip out any encryption metadata block. The reason we need to do this is;
                    // Any encrypted data we encounter in the whole file will be decoded, decrypted and replaced.
                    // The data will be decrypted using the machine level protection scope.
                    // The encyption metadata itself contains two encrypted settings, one is encrypted at machine-level scope
                    // the other however is encrypted at user-level scope. The decryption tool uses this as a check to ensure 
                    // only the encrypting user can decrypt the settings file. So if we try to decrypt this mix of encryption
                    // scopes with only machine scope it will fail.
                    // Therefore we remove the encryption block.
                    int pos = jsonContent.IndexOf("\"EncryptedBy\":");

                    if ((pos >= 0) &&
                        jsonContent.Contains("\"DecryptedAt\":") &&
                        jsonContent.Contains("\"EncryptedAt\":"))
                    {
                        string preMetadata = jsonContent.Substring(0, pos);
                        pos = preMetadata.LastIndexOf(",");

                        if (pos >= 0)
                        {
                            // if a comma exists (and assuming we start off with valid JSON) then it means the metadata comes after another block, so we effectively strip off the comma.
                            preMetadata = preMetadata.Substring(0, pos);
                        }

                        pos = jsonContent.IndexOf("}", pos);
                        string postMetadata = jsonContent.Substring(pos + 1);

                        jsonContent = $"{preMetadata}{postMetadata}";
                    }

                    jsonContent = Settings.Decrypt(jsonContent, new DpApiSettingsEncryptor(DpApiSettingsEncryptor.ProtectionScope.Machine), AdditionalEntropy.CurrentValue);
                    AdditionalEntropy.Reset();
                }

                // Now pass the (decoded?) content on to the base implementation so it's unaware of the encryption!
                using (Stream decodedStream = StreamUtils.StringStream(jsonContent))
                {
                    base.Load(decodedStream);
                }
            }
        }
    }
}
