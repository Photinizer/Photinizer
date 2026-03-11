using Photinizer.Builder;

namespace Photinizer.OwnUI.Minimal;

public class MyAmazingApplication : Application
{
    public MyAmazingApplication(IServiceProvider services, IAppEnvironment environment) : base(services)
    {
        // logging registration must go to the builder
        //throw new Exception(environment.EnvironmentName);
        Console.WriteLine(environment.EnvironmentName);
    }
}
