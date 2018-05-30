using System;
using System.Collections.Concurrent;
using System.Net.Http;
using KubeClient.ResourceClients;
using Microsoft.Extensions.Options;

namespace KubeClient
{
    /// <summary>
    ///     Client for the Kubernetes API.
    /// </summary>
    public sealed class KubeApiClient
        : IKubeApiClient
    {
        /// <summary>
        ///     Kubernetes resource clients.
        /// </summary>
        private readonly ConcurrentDictionary<Type, IKubeResourceClient> _clients = new ConcurrentDictionary<Type, IKubeResourceClient>();
        private readonly KubeClientOptions _options;

        /// <summary>
        ///     Create a new <see cref="KubeApiClient"/>.
        /// </summary>
        /// <param name="httpClientFactory">
        ///     Factory for creating the underlying HTTP client.
        /// </param>
        /// <param name="options">
        ///     The <see cref="KubeClientOptions"/> used to configure the <see cref="KubeApiClient"/>.
        /// </param>
        public KubeApiClient(IOptions<KubeClientOptions> options, IHttpClientFactory httpClientFactory)
        {
            _options = options.Value.Clone();
            Http = httpClientFactory.Create(_options);

            DefaultNamespace = _options.KubeNamespace;
        }

        /// <summary>
        ///     Dispose of resources being used by the <see cref="T:KubeClient.KubeApiClient" />.
        /// </summary>
        public void Dispose() => Http.Dispose();

        /// <summary>
        ///     The base address of the Kubernetes API end-point targeted by the client.
        /// </summary>
        public Uri ApiEndPoint => _options.ApiEndPoint;

        /// <summary>
        ///     The default Kubernetes namespace.
        /// </summary>
        public string DefaultNamespace { get; set; }

        /// <summary>
        ///     The underlying HTTP client.
        /// </summary>
        public HttpClient Http { get; }

        /// <summary>
        ///     Get a copy of the <see cref="KubeClientOptions"/> used to configure the client.
        /// </summary>
        /// <returns>
        ///     The <see cref="KubeClientOptions"/>.
        /// </returns>
        public KubeClientOptions GetClientOptions() => _options.Clone();

        /// <summary>
        ///     Get or create a Kubernetes resource client of the specified type.
        /// </summary>
        /// <typeparam name="TResourceClient">
        ///     The type of Kubernetes resource client to get or create.
        /// </typeparam>
        /// <param name="resourceClientFactory">
        ///     A delegate that creates the resource client.
        /// </param>
        /// <returns>
        ///     The resource client.
        /// </returns>
        public TResourceClient ResourceClient<TResourceClient>(Func<IKubeApiClient, TResourceClient> resourceClientFactory)
            where TResourceClient : IKubeResourceClient
        {
            if (resourceClientFactory == null)
                throw new ArgumentNullException(nameof(resourceClientFactory));

            return (TResourceClient)_clients.GetOrAdd(typeof(TResourceClient), clientType =>
            {
                TResourceClient resourceClient = resourceClientFactory(this);
                if (resourceClient == null)
                    throw new InvalidOperationException($"Factory for Kubernetes resource client of type '{clientType.FullName}' returned null.");

                return (IKubeResourceClient)resourceClient;
            });
        }
    }
}