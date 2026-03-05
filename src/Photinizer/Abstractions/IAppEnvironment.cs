namespace Photinizer
{
    /// <summary>
    /// Provides information about the application environment an application is running in.
    /// </summary>
    public interface IAppEnvironment
    {
        /// <summary>
        /// Gets or sets the name of the environment. The app builder automatically sets this property to the value of the
        /// "environment" key as specified in configuration.
        /// </summary>
        string EnvironmentName { get; set; }

        /// <summary>
        /// Gets or sets the name of the application. This property is automatically set by the app builder to the assembly containing
        /// the application entry point.
        /// </summary>
        string ApplicationName { get; set; }

        /// <summary>
        /// Gets or sets the absolute path to the directory that contains the application content files.
        /// </summary>
        string ContentRootPath { get; set; }
    }
}
