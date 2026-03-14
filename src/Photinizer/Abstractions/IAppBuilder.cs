using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Photinizer.Builder;

namespace Photinizer;

public interface IAppBuilder
{
    /// <summary>
    /// Provides information about the application environment.
    /// </summary>
    IAppEnvironment Environment { get; }

    /// <summary>
    /// A collection of configuration providers for the application to compose.
    /// </summary>
    IConfigurationManager Configuration { get; }

    /// <summary>
    /// A collection of services for the application to compose.
    /// </summary>
    IServiceCollection Services { get; }

    /// <summary>
    /// A collection of logging providers for the application to compose. This is useful for adding new logging providers.
    /// </summary>
    ILoggingBuilder Logging { get; }

    /// <summary>
    /// Builds the Application.
    /// </summary>
    /// <returns>A configured Application.</returns>
    Application Build();
}