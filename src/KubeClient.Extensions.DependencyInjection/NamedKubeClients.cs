using Microsoft.Extensions.Options;
using System;

namespace KubeClient
{
    /// <summary>
    ///     A service for resolving named Kubernetes clients.
    /// </summary>
    public class NamedKubeClients
        : INamedKubeClients
    {
        private readonly IOptionsMonitor<KubeClientOptions> _options;
        private readonly IKubeApiClientFactory _apiClientFactory;

        /// <summary>
        ///     Create a new <see cref="NamedKubeClients"/> service.
        /// </summary>
        /// <param name="options">
        ///     The <see cref="KubeClientOptions"/>. 
        /// </param>
        /// <param name="apiClientFactory">
        ///     Factory for creating <see cref="IKubeApiClient"/> on demand.
        /// </param>
        public NamedKubeClients(IOptionsMonitor<KubeClientOptions> options, IKubeApiClientFactory apiClientFactory)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            _options = options;
            _apiClientFactory = apiClientFactory;
        }

        /// <summary>
        ///     Resolve the Kubernetes API client with the specified name.
        /// </summary>
        /// <param name="name">
        ///     The client name.
        /// </param>
        /// <returns>
        ///     The resolved <see cref="KubeApiClient"/>.
        /// </returns>
        public IKubeApiClient Get(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'name'.", nameof(name));

            KubeClientOptions options = _options.Get(name);
            if (options == null)
                throw new InvalidOperationException($"Cannot resolve a {nameof(KubeClientOptions)} instance named '{name}'.");

            return _apiClientFactory.Create(options);
        }
    }
}