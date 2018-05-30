namespace KubeClient
{
    /// <summary>
    ///     Factory for creating <see cref="IKubeApiClient"/>s.
    /// </summary>
    public interface IKubeApiClientFactory
    {
        /// <summary>
        ///     Creates a new <see cref="IKubeApiClient"/> based on <see cref="KubeClientOptions"/>.
        /// </summary>
        /// <param name="options">
        ///     The options to use for the <see cref="IKubeApiClient"/>.
        /// </param>
        /// <returns>The created <see cref="IKubeApiClient"/>.</returns>
        IKubeApiClient Create(KubeClientOptions options);
    }
}