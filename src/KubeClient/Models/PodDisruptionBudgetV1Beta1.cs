using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace KubeClient.Models
{
    /// <summary>
    ///     PodDisruptionBudget is an object to define the max disruption that can be caused to a collection of pods
    /// </summary>
    [KubeObject("PodDisruptionBudget", "policy/v1beta1")]
    public partial class PodDisruptionBudgetV1Beta1 : KubeResourceV1
    {
        /// <summary>
        ///     Specification of the desired behavior of the PodDisruptionBudget.
        /// </summary>
        [JsonProperty("spec")]
        public PodDisruptionBudgetSpecV1Beta1 Spec { get; set; }

        /// <summary>
        ///     Most recently observed status of the PodDisruptionBudget.
        /// </summary>
        [JsonProperty("status")]
        public PodDisruptionBudgetStatusV1Beta1 Status { get; set; }
    }
}
