using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Sino.RestBus.Client.Http.Formatting
{
    /// <summary>
    /// Represents the default <see cref="IContractResolver"/> used by <see cref="BaseJsonMediaTypeFormatter"/>.
    /// It uses the formatter's <see cref="IRequiredMemberSelector"/> to select required members and recognizes
    /// the <see cref="SerializableAttribute"/> type annotation.
    /// </summary>
    internal class JsonContractResolver : DefaultContractResolver
    {
        private readonly MediaTypeFormatter _formatter;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonContractResolver" /> class.
        /// </summary>
        /// <param name="formatter">The formatter to use for resolving required members.</param>
        public JsonContractResolver(MediaTypeFormatter formatter)
        {
            if (formatter == null)
            {
                throw Error.ArgumentNull("formatter");
            }

            _formatter = formatter;
            // Need this setting to have [Serializable] types serialized correctly
            IgnoreSerializableAttribute = false;
        }

        // Determines whether a member is required or not and sets the appropriate JsonProperty settings
        private void ConfigureProperty(MemberInfo member, JsonProperty property)
        {
            if (_formatter.RequiredMemberSelector != null && _formatter.RequiredMemberSelector.IsRequiredMember(member))
            {
                property.Required = Required.AllowNull;
                property.DefaultValueHandling = DefaultValueHandling.Include;
                property.NullValueHandling = NullValueHandling.Include;
            }
            else
            {
                property.Required = Required.Default;
            }
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);
            ConfigureProperty(member, property);
            return property;
        }
    }
}
