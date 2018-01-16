using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System;
using Newtonsoft.Json;

namespace Sino.RestBus.Client.Http.Formatting
{
    /// <summary>
    /// <see cref="MediaTypeFormatter"/> class to handle Json.
    /// </summary>
    internal class JsonMediaTypeFormatter : BaseJsonMediaTypeFormatter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonMediaTypeFormatter"/> class.
        /// </summary>
        public JsonMediaTypeFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeConstants.ApplicationJsonMediaType);
            SupportedMediaTypes.Add(MediaTypeConstants.TextJsonMediaType);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonMediaTypeFormatter"/> class.
        /// </summary>
        /// <param name="formatter">The <see cref="JsonMediaTypeFormatter"/> instance to copy settings from.</param>
        protected JsonMediaTypeFormatter(JsonMediaTypeFormatter formatter)
            : base(formatter)
        {
            UseDataContractJsonSerializer = formatter.UseDataContractJsonSerializer;
            Indent = formatter.Indent;
        }

        /// <summary>
        /// Gets the default media type for Json, namely "application/json".
        /// </summary>
        /// <remarks>
        /// The default media type does not have any <c>charset</c> parameter as
        /// the <see cref="Encoding"/> can be configured on a per <see cref="JsonMediaTypeFormatter"/>
        /// instance basis.
        /// </remarks>
        /// <value>
        /// Because <see cref="MediaTypeHeaderValue"/> is mutable, the value
        /// returned will be a new instance every time.
        /// </value>
        public static MediaTypeHeaderValue DefaultMediaType
        {
            get { return MediaTypeConstants.ApplicationJsonMediaType; }
        }

        public bool UseDataContractJsonSerializer { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to indent elements when writing data. 
        /// </summary>
        public bool Indent { get; set; }

        public override JsonReader CreateJsonReader(Type type, Stream readStream, Encoding effectiveEncoding)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            if (readStream == null)
            {
                throw Error.ArgumentNull("readStream");
            }

            if (effectiveEncoding == null)
            {
                throw Error.ArgumentNull("effectiveEncoding");
            }

            return new JsonTextReader(new StreamReader(readStream, effectiveEncoding));
        }

        public override JsonWriter CreateJsonWriter(Type type, Stream writeStream, Encoding effectiveEncoding)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            if (writeStream == null)
            {
                throw Error.ArgumentNull("writeStream");
            }

            if (effectiveEncoding == null)
            {
                throw Error.ArgumentNull("effectiveEncoding");
            }

            JsonWriter jsonWriter = new JsonTextWriter(new StreamWriter(writeStream, effectiveEncoding));
            if (Indent)
            {
                jsonWriter.Formatting = Newtonsoft.Json.Formatting.Indented;
            }

            return jsonWriter;
        }
    }
}