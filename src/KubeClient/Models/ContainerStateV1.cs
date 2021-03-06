using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace KubeClient.Models
{
    /// <summary>
    ///     ContainerState holds a possible state of container. Only one of its members may be specified. If none of them is specified, the default one is ContainerStateWaiting.
    /// </summary>
    public partial class ContainerStateV1
    {
        /// <summary>
        ///     Details about a terminated container
        /// </summary>
        [JsonProperty("terminated")]
        public ContainerStateTerminatedV1 Terminated { get; set; }

        /// <summary>
        ///     Details about a waiting container
        /// </summary>
        [JsonProperty("waiting")]
        public ContainerStateWaitingV1 Waiting { get; set; }

        /// <summary>
        ///     Details about a running container
        /// </summary>
        [JsonProperty("running")]
        public ContainerStateRunningV1 Running { get; set; }
    }
}
