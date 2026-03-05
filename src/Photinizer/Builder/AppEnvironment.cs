namespace Photinizer.Builder
{
    internal sealed class AppEnvironment : IAppEnvironment
    {
        /// <inheritdoc />
        public string EnvironmentName { get; set; } = null!;
        /// <inheritdoc />
        public string ApplicationName { get; set; } = null!;
        /// <inheritdoc />
        public string ContentRootPath { get; set; } = null!;
    }
}
