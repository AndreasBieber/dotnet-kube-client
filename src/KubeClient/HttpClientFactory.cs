using System;
using System.Net.Http;
using HTTPlease;
using HTTPlease.Diagnostics;
using KubeClient.MessageHandlers;
using Microsoft.Extensions.Logging;

namespace KubeClient
{
    /// <summary>
    ///     Factory for creating <see cref="HttpClient"/>s.
    /// </summary>
    public class HttpClientFactory : IHttpClientFactory
    {
        private readonly ILoggerFactory _loggerFactory;

        /// <summary>
        ///     Creates a new <see cref="HttpClientFactory"/>.
        /// </summary>
        /// <param name="loggerFactory">
        ///     The <see cref="ILoggerFactory"/> to use for http logging.
        /// </param>
        public HttpClientFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        /// <summary>
        ///     Creates a new <see cref="HttpClient"/> based on the given <see cref="KubeClientOptions"/>.
        /// </summary>
        /// <remarks>
        ///     Configures <c>BearerToken authentication</c>, <c>Client certificate authentication</c>, <c>Server certificate validation</c> and <c>Logging</c>
        ///     based on <see cref="KubeClientOptions"/>.
        /// </remarks>
        /// <param name="options">
        ///     The <see cref="KubeClientOptions"/> to use.
        /// </param>
        /// <returns>The created <see cref="HttpClient"/>.</returns>
        public HttpClient Create(KubeClientOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            options.EnsureValid();

            var clientBuilder = new ClientBuilder();
            ConfgureBearerToken(options, ref clientBuilder);
            ConfigureCertificateAuthority(options, ref clientBuilder);
            ConfigureClientCertificate(options, ref clientBuilder);
            ConfigureLogging(options, ref clientBuilder);

            return clientBuilder.CreateClient();
        }

        private static void ConfgureBearerToken(KubeClientOptions options, ref ClientBuilder clientBuilder)
        {
            if (!String.IsNullOrWhiteSpace(options.AccessToken))
            {
                clientBuilder = clientBuilder.AddHandler(
                    () => new BearerTokenHandler(options.AccessToken)
                );
            }
        }

        private static void ConfigureCertificateAuthority(KubeClientOptions options, ref ClientBuilder clientBuilder)
        {
            if (options.AllowInsecure)
                clientBuilder = clientBuilder.AcceptAnyServerCertificate();
            else if (options.CertificationAuthorityCertificate != null)
                clientBuilder = clientBuilder.WithServerCertificate(options.CertificationAuthorityCertificate);
        }

        private static void ConfigureClientCertificate(KubeClientOptions options, ref ClientBuilder clientBuilder)
        {
            if (options.ClientCertificate != null)
                clientBuilder = clientBuilder.WithClientCertificate(options.ClientCertificate);
        }

        private void ConfigureLogging(KubeClientOptions options, ref ClientBuilder clientBuilder)
        {
            if (options.LogHeaders || options.LogPayloads)
            {
                var logComponents = LogMessageComponents.Basic;
                ConfigureHttpHeaderLogging(options, ref logComponents);
                ConfigureHttpPayloadLogging(options, ref logComponents);
                InstructClientBuilderToLog(ref clientBuilder, logComponents);
            }
        }

        private static void ConfigureHttpPayloadLogging(KubeClientOptions options, ref LogMessageComponents logComponents)
        {
            if (options.LogPayloads)
                logComponents |= LogMessageComponents.Body;
        }

        private static void ConfigureHttpHeaderLogging(KubeClientOptions options, ref LogMessageComponents logComponents)
        {
            if (options.LogHeaders)
                logComponents |= LogMessageComponents.Headers;
        }

        private void InstructClientBuilderToLog(ref ClientBuilder clientBuilder, LogMessageComponents logComponents)
        {
            clientBuilder = clientBuilder.WithLogging(
                _loggerFactory.CreateLogger(typeof(HttpClient).FullName),
                requestComponents: logComponents,
                responseComponents: logComponents
            );
        }
    }
}