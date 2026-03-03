namespace Photinizer.Builder;

public class PhotinizerBuildOptions
{
    private readonly Dictionary<string, string>? _args;

    private const string BuildSourceArg = "--build-source";

    public PhotinizerBuildOptions(string[] args)
    {
        if (args is not { Length: > 0 })
        {
            args = Environment.GetCommandLineArgs();
        }

        bool isBuildMode = false;
        foreach (var arg in args)
        {
            if (arg.Equals(BuildSourceArg, StringComparison.Ordinal))
            {
                isBuildMode = true;
                continue;
            }

            if (isBuildMode)
            {
                (_args ??= new(StringComparer.Ordinal)).Add(BuildSourceArg, arg);
                isBuildMode = false;
                IsBuildMode = true;
                continue;
            }

            // add other build options
        }
    }

    public bool IsBuildMode { get; }

    public string BuildSource => field ??= GetBuildSource();

    private string GetBuildSource() => IsBuildMode ? _args![BuildSourceArg] : string.Empty;
}