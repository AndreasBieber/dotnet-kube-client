using HTTPlease;
using HTTPlease.Diagnostics;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;

namespace KubeClient
{
    using MessageHandlers;
    using ResourceClients;

    /// <summary>
    ///     Client for the Kubernetes API.
    /// </summary>
    public sealed class KubeApiClient
        : IDisposable
    {
        /// <summary>
        ///     Kubernetes resource clients.
        /// </summary>
        readonly ConcurrentDictionary<Type, KubeResourceClient> _clients = new ConcurrentDictionary<Type, KubeResourceClient>();

        /// <summary>
        ///     Create a new <see cref="KubeApiClient"/>.
        /// </summary>
        /// <param name="httpClient">
        ///     The underlying HTTP client.
        /// </param>
        /// <param name="options">
        ///     The <see cref="KubeClientOptions"/> used to configure the <see cref="KubeApiClient"/>.
        /// </param>
        /// <param name="loggerFactory">
        ///     The <see cref="ILoggerFactory"/> used to create loggers for client components, or null to use a no-op logger factory.
        /// </param>
        KubeApiClient(HttpClient httpClient, KubeClientOptions options, ILoggerFactory loggerFactory)
        {
            if (httpClient == null)
                throw new ArgumentNullException(nameof(httpClient));

            Http = httpClient;
            Options = options.Clone();
            LoggerFactory = loggerFactory ?? new LoggerFactory();

            DefaultNamespace = options.KubeNamespace;
        }

        /// <summary>
        ///     Dispose of resources being used by the <see cref="T:KubeClient.KubeApiClient" />.
        /// </summary>
        public void Dispose() => Http.Dispose();

        /// <summary>
        ///     The default Kubernetes namespace.
        /// </summary>
        public string DefaultNamespace { get; set; }

        /// <summary>
        ///     The base address of the Kubernetes API end-point targeted by the client.
        /// </summary>
        public Uri ApiEndPoint => Options.ApiEndPoint;

        /// <summary>
        ///     The underlying HTTP client.
        /// </summary>
        internal HttpClient Http { get; }

        /// <summary>
        ///     The <see cref="ILoggerFactory"/> used to create loggers for client components.
        /// </summary>
        internal ILoggerFactory LoggerFactory { get; }

        /// <summary>
        ///     The <see cref="KubeClientOptions"/> used to configure the <see cref="KubeApiClient"/>.
        /// </summary>
        KubeClientOptions Options { get; }

        /// <summary>
        ///     Get a copy of the <see cref="KubeClientOptions"/> used to configure the client.
        /// </summary>
        /// <returns>
        ///     The <see cref="KubeClientOptions"/>.
        /// </returns>
        public KubeClientOptions GetClientOptions() => Options.Clone();

        /// <summary>
        ///     Get or create a Kubernetes resource client of the specified type.
        /// </summary>
        /// <typeparam name="TClient">
        ///     The type of Kubernetes resource client to get or create.
        /// </typeparam>
        /// <param name="clientFactory">
        ///     A delegate that creates the resource client.
        /// </param>
        /// <returns>
        ///     The resource client.
        /// </returns>
        public TClient ResourceClient<TClient>(Func<KubeApiClient, TClient> clientFactory)
            where TClient : KubeResourceClient
        {
            if (clientFactory == null)
                throw new ArgumentNullException(nameof(clientFactory));

            return (TClient)_clients.GetOrAdd(typeof(TClient), clientType =>
            {
                TClient resourceClient = clientFactory(this);
                if (resourceClient == null)
                    throw new InvalidOperationException($"Factory for Kubernetes resource client of type '{clientType.FullName}' returned null.");

                return (KubeResourceClient)resourceClient;
            });
        }

        /// <summary>
        ///     Create and configure a <see cref="KubeApiClient"/> using the specified options.
        /// </summary>
        /// <param name="options">
        ///     The <see cref="KubeClientOptions"/> used to configure the client.
        /// </param>
        /// <param name="loggerFactory">
        ///     An optional <see cref="ILoggerFactory"/> used to create loggers for client components.
        /// </param>
        /// <returns>
        ///     The configured <see cref="KubeApiClient"/>.
        /// </returns>
        public static KubeApiClient Create(KubeClientOptions options, ILoggerFactory loggerFactory = null)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            options.EnsureValid();
            
            var clientBuilder = new ClientBuilder();

            if (!String.IsNullOrWhiteSpace(options.AccessToken))
            {
                clientBuilder = clientBuilder.AddHandler(
                    () => new BearerTokenHandler(options.AccessToken)
                );
            }

            if (options.AllowInsecure)
                clientBuilder = clientBuilder.AcceptAnyServerCertificate();
            else if (options.CertificationAuthorityCertificate != null)
                clientBuilder = clientBuilder.WithServerCertificate(options.CertificationAuthorityCertificate);

            if (options.ClientCertificate != null)
                clientBuilder = clientBuilder.WithClientCertificate(options.ClientCertificate);

            if (loggerFactory != null)
            {
                LogMessageComponents logComponents = LogMessageComponents.Basic;
                if (options.LogHeaders)
                    logComponents |= LogMessageComponents.Headers;
                if (options.LogPayloads)
                    logComponents |= LogMessageComponents.Body;

                clientBuilder = clientBuilder.WithLogging(
                    logger: loggerFactory.CreateLogger(
                        typeof(KubeApiClient).FullName + ".Http"
                    ),
                    requestComponents: logComponents,
                    responseComponents: logComponents
                );
            }

            HttpClient httpClient = clientBuilder.CreateClient(options.ApiEndPoint);

            return new KubeApiClient(httpClient, options, loggerFactory);
        }

        /// <summary>
        ///     Create a new <see cref="KubeApiClient"/> without authentication.
        /// </summary>
        /// <param name="apiEndPoint">
        ///     The base address for the Kubernetes API end-point.
        /// </param>
        /// <param name="loggerFactory">
        ///     An optional <see cref="ILoggerFactory"/> used to create loggers for client components.
        /// </param>
        /// <returns>
        ///     The configured <see cref="KubeApiClient"/>.
        /// </returns>
        public static KubeApiClient Create(string apiEndPoint, ILoggerFactory loggerFactory = null)
        {
            return Create(new Uri(apiEndPoint), loggerFactory);
        }

        /// <summary>
        ///     Create a new <see cref="KubeApiClient"/> without authentication.
        /// </summary>
        /// <param name="apiEndPoint">
        ///     The base address for the Kubernetes API end-point.
        /// </param>
        /// <param name="loggerFactory">
        ///     An optional <see cref="ILoggerFactory"/> used to create loggers for client components.
        /// </param>
        /// <returns>
        ///     The configured <see cref="KubeApiClient"/>.
        /// </returns>
        public static KubeApiClient Create(Uri apiEndPoint, ILoggerFactory loggerFactory = null)
        {
            return Create(new KubeClientOptions
            {
                ApiEndPoint = apiEndPoint
            }, loggerFactory);
        }

        /// <summary>
        ///     Create a new <see cref="KubeApiClient"/> using a bearer token for authentication.
        /// </summary>
        /// <param name="apiEndPoint">
        ///     The base address for the Kubernetes API end-point.
        /// </param>
        /// <param name="accessToken">
        ///     The access token to use for authentication to the API.
        /// </param>
        /// <param name="expectServerCertificate">
        ///     An optional server certificate to expect.
        /// </param>
        /// <param name="loggerFactory">
        ///     An optional <see cref="ILoggerFactory"/> used to create loggers for client components.
        /// </param>
        /// <returns>
        ///     The configured <see cref="KubeApiClient"/>.
        /// </returns>
        public static KubeApiClient Create(string apiEndPoint, string accessToken, X509Certificate2 expectServerCertificate = null, ILoggerFactory loggerFactory = null)
        {
            return Create(new KubeClientOptions
            {
                ApiEndPoint = new Uri(apiEndPoint),
                AccessToken = accessToken,
                CertificationAuthorityCertificate = expectServerCertificate
            }, loggerFactory);
        }

        /// <summary>
        ///     Create a new <see cref="KubeApiClient"/> using an X.509 certificate for client authentication.
        /// </summary>
        /// <param name="apiEndPoint">
        ///     The base address for the Kubernetes API end-point.
        /// </param>
        /// <param name="clientCertificate">
        ///     The X.509 certificate to use for client authentication.
        /// </param>
        /// <param name="expectServerCertificate">
        ///     An optional server certificate to expect.
        /// </param>
        /// <param name="loggerFactory">
        ///     An optional <see cref="ILoggerFactory"/> used to create loggers for client components.
        /// </param>
        /// <returns>
        ///     The configured <see cref="KubeApiClient"/>.
        /// </returns>
        public static KubeApiClient Create(string apiEndPoint, X509Certificate2 clientCertificate, X509Certificate2 expectServerCertificate = null, ILoggerFactory loggerFactory = null)
        {
            return Create(new KubeClientOptions
            {
                ApiEndPoint = new Uri(apiEndPoint),
                ClientCertificate = clientCertificate,
                CertificationAuthorityCertificate = expectServerCertificate
            }, loggerFactory);
        }

        /// <summary>
        ///     Create a new <see cref="KubeApiClient"/> using pod-level configuration.
        /// </summary>
        /// <param name="loggerFactory">
        ///     An optional <see cref="ILoggerFactory"/> used to create loggers for client components.
        /// </param>
        /// <returns>
        ///     The configured <see cref="KubeApiClient"/>.
        /// </returns>
        /// <remarks>
        ///     Only works from within a container running in a Kubernetes Pod.
        /// </remarks>
        public static KubeApiClient CreateFromPodServiceAccount(ILoggerFactory loggerFactory = null)
        {
            return Create(
                KubeClientOptions.FromPodServiceAccount(),
                loggerFactory
            );
        }

        /// <summary>
        ///     Create a new <see cref="KubeApiClient"/> (for testing purposes only).
        /// </summary>
        /// <param name="httpClient">
        ///     The <see cref="HttpClient"/> used to communicate with the Kubernetes API.
        /// </param>
        /// <param name="options">
        ///     The <see cref="KubeClientOptions"/> used to configure the <see cref="KubeApiClient"/>.
        /// </param>
        /// <param name="loggerFactory">
        ///     An optional <see cref="ILoggerFactory"/> used to create loggers for client components.
        /// </param>
        /// <returns>
        ///     The configured <see cref="KubeApiClient"/>.
        /// </returns>
        internal static KubeApiClient Create(HttpClient httpClient, KubeClientOptions options, ILoggerFactory loggerFactory = null)
        {
            if (httpClient == null)
                throw new ArgumentNullException(nameof(httpClient));

            if (options == null)
                throw new ArgumentNullException(nameof(options));
            
            if (httpClient.BaseAddress == null)
                throw new ArgumentException("The KubeApiClient's underlying HttpClient must specify a base address.", nameof(httpClient));

            options.EnsureValid();

            return new KubeApiClient(httpClient, options, loggerFactory);
        }
    }
}