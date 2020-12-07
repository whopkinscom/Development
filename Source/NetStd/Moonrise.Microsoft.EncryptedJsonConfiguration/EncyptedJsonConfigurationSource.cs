using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace Moonrise.Microsoft.Extensions.Configuration.EncryptedJsonConfiguration
{
    /// <summary>
    ///     Represents a potentially encrypted JSON file as an <see cref="IConfigurationSource" />.
    /// </summary>
    /// <seealso cref="Microsoft.Extensions.Configuration.Json.JsonConfigurationSource" />
    public class EncyptedJsonConfigurationSource : JsonConfigurationSource
    {
        /// <summary>
        ///     Builds the <see cref="T:Microsoft.Extensions.Configuration.Json.JsonConfigurationProvider" /> for this source.
        /// </summary>
        /// <param name="builder">The <see cref="T:Microsoft.Extensions.Configuration.IConfigurationBuilder" />.</param>
        /// <returns>
        ///     A <see cref="EncryptedJsonConfigurationProvider" />
        /// </returns>
        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            EnsureDefaults(builder);
            return new EncryptedJsonConfigurationProvider(this);
        }
    }
}
