using Microsoft.Extensions.Options;

namespace KubeClient
{
    /// <summary>
    ///     Factory for creating <see cref="IKubeApiClient"/>s.
    /// </summary>
    public class KubeApiClientFactory 
        : IKubeApiClientFactory
    {
        private readonly IHttpClientFactory _httpClientFactory;

        /// <summary>
        ///     Creates a new <see cref="KubeApiClientFactory"/>.
        /// </summary>
        /// <param name="httpClientFactory">
        ///     The <see cref="IHttpClientFactory"/> to use.
        /// </param>
        public KubeApiClientFactory(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        ///     Creates a new <see cref="KubeApiClient"/> based on <see cref="KubeClientOptions"/>.
        /// </summary>
        /// <param name="options">
        ///     The options to use for the <see cref="IKubeApiClient"/>.
        /// </param>
        /// <returns>The created <see cref="KubeApiClient"/>.</returns>
        public IKubeApiClient Create(KubeClientOptions options) 
            => new KubeApiClient(new OptionsWrapper<KubeClientOptions>(options), _httpClientFactory);
    }
}