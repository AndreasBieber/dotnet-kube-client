using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace KubeClient.Models
{
    /// <summary>
    ///     ObjectFieldSelector selects an APIVersioned field of an object.
    /// </summary>
    public partial class ObjectFieldSelectorV1
    {
        /// <summary>
        ///     Version of the schema the FieldPath is written in terms of, defaults to "v1".
        /// </summary>
        [JsonProperty("apiVersion")]
        public string ApiVersion { get; set; }

        /// <summary>
        ///     Path of the field to select in the specified API version.
        /// </summary>
        [JsonProperty("fieldPath")]
        public string FieldPath { get; set; }
    }
}
