namespace Photinizer.Builder;

public static class AppWindowsExtensions
{
    extension(Application app)
    {
        public string ResolveSourcePath(string source)
        {
            var webRoot = app.Configuration[ConfigurationDefaults.WebRootKey];
            if (!string.IsNullOrWhiteSpace(webRoot))
            {
                return Path.Combine(webRoot, source);
            }
            return Path.Combine(app.Environment.ContentRootPath, "wwwroot", source);
        }
    }
}
