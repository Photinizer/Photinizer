namespace Photinizer.Builder
{
    public static class AppEnvironmentExtensions
    {
        extension(IAppEnvironment appEnvironment)
        {
            /// <summary>
            /// Checks if the current host environment name is <see cref="Environments.Development"/>.
            /// </summary>
            /// <returns><see langword="true"/> if the environment name is <see cref="Environments.Development"/>, otherwise <see langword="false"/>.</returns>
            public bool IsDevelopment()
            {
                ArgumentNullException.ThrowIfNull(appEnvironment);
                return appEnvironment.IsEnvironment(Environments.Development);
            }

            /// <summary>
            /// Checks if the current host environment name is <see cref="Environments.Staging"/>.
            /// </summary>
            /// <returns><see langword="true"/> if the environment name is <see cref="Environments.Staging"/>, otherwise <see langword="false"/>.</returns>
            public bool IsStaging()
            {
                ArgumentNullException.ThrowIfNull(appEnvironment);
                return appEnvironment.IsEnvironment(Environments.Staging);
            }

            /// <summary>
            /// Checks if the current host environment name is <see cref="Environments.Production"/>.
            /// </summary>
            /// <returns><see langword="true"/> if the environment name is <see cref="Environments.Production"/>, otherwise <see langword="false"/>.</returns>
            public bool IsProduction()
            {
                ArgumentNullException.ThrowIfNull(appEnvironment);
                return appEnvironment.IsEnvironment(Environments.Production);
            }

            /// <summary>
            /// Compares the current host environment name against the specified value.
            /// </summary>
            /// <param name="environmentName">Environment name to validate against.</param>
            /// <returns><see langword="true"/> if the specified name is the same as the current environment, otherwise <see langword="false"/>.</returns>
            public bool IsEnvironment(string environmentName)
            {
                ArgumentNullException.ThrowIfNull(appEnvironment);
                return string.Equals(appEnvironment.EnvironmentName, environmentName, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
