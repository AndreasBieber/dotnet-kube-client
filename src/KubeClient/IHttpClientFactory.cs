using System.Net.Http;

namespace KubeClient
{
    /// <summary>
    ///     Factory for creating <see cref="HttpClient"/>s.
    /// </summary>
    public interface IHttpClientFactory
    {
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
        HttpClient Create(KubeClientOptions options);
    }
}