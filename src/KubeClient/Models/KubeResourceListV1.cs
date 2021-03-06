using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;

namespace KubeClient.Models
{
    /// <summary>
    ///     The base class for Kubernetes resource lists.
    /// </summary>
    public abstract class KubeResourceListV1
        : KubeObjectV1
    {
        /// <summary>
        ///     Model type metadata.
        /// </summary>
        static readonly ConcurrentDictionary<Type, (string kind, string apiVersion)> ItemModelMetadata = new ConcurrentDictionary<Type, (string kind, string apiVersion)>();

        /// <summary>
        ///     Standard list metadata. More info: https://git.k8s.io/community/contributors/devel/api-conventions.md#metadata
        /// </summary>
        [JsonProperty("metadata")]
        public ListMetaV1 Metadata { get; set; }

        /// <summary>
        ///     Get Kubernetes Kind / ApiVersion metadata for the items contained by the specified list type.
        /// </summary>
        /// <typeparam name="TResourceList">
        ///     The target resource-list type.
        /// </typeparam>
        /// <returns>
        ///     A tuple containing the item Kind and ApiVersion metadata (or <c>null</c> and <c>null</c>, if no item metadata is available for <typeparamref name="TResourceList"/>).
        /// </returns>
        public static (string kind, string apiVersion) GetListItemKubeKind<TResourceList>()
            where TResourceList : KubeResourceListV1
        {
            (string kind, string apiVersion) = ItemModelMetadata.GetOrAdd(typeof(TResourceList), modelType =>
            {
                var kubeListItemAttribute = modelType.GetTypeInfo().GetCustomAttribute<KubeListItemAttribute>();
                if (kubeListItemAttribute != null)
                    return (kubeListItemAttribute.Kind, kubeListItemAttribute.ApiVersion);

                return (null, null);
            });

            return (kind, apiVersion);
        }
    }

    /// <summary>
    ///     The base class for Kubernetes resource lists where the resource type is known.
    /// </summary>
    /// <typeparam name="TResource">
    ///     The type of resource contained in the list.
    /// </typeparam>
    public abstract class KubeResourceListV1<TResource>
        : KubeResourceListV1, IEnumerable<TResource>
    {
        /// <summary>
        ///     The list's resources.
        /// </summary>
        public abstract List<TResource> Items { get; }

        /// <summary>
        ///     Get a typed enumerator for the list's resources.
        /// </summary>
        /// <returns>
        ///     The <see cref="IEnumerator{T}"/>.
        /// </returns>
        public IEnumerator<TResource> GetEnumerator() => Items.GetEnumerator();

        /// <summary>
        ///     Get an untyped enumerator for the list's resources.
        /// </summary>
        /// <returns>
        ///     The <see cref="IEnumerator"/>.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
