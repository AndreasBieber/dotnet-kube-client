using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace KubeClient.Models
{
    /// <summary>
    ///     HostAlias holds the mapping between IP and hostnames that will be injected as an entry in the pod's hosts file.
    /// </summary>
    public partial class HostAliasV1
    {
        /// <summary>
        ///     Hostnames for the above IP address.
        /// </summary>
        [JsonProperty("hostnames", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Hostnames { get; set; } = new List<string>();

        /// <summary>
        ///     IP address of the host file entry.
        /// </summary>
        [JsonProperty("ip")]
        public string Ip { get; set; }
    }
}
