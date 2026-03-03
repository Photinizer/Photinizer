using System.Text.RegularExpressions;

namespace Photinizer.Settings;

public partial class PhotinizerBuildSettings
{
    private readonly string _args;

    private const string BuildSouceArg = "--build-source";

    public PhotinizerBuildSettings()
    {
        _args = string.Join(" ", Environment.GetCommandLineArgs());
        IsBuildMode = _args.Contains(BuildSouceArg);
    }

    public bool IsBuildMode { get; private set; }

    public string BuildSource => field ??= GetBuildSource();

    private string GetBuildSource()
        => s_BuildSource().Match(_args).Groups[1].Value;

    [GeneratedRegex("--build-source=\"(.+?)\"")]
    private static partial Regex s_BuildSource();
}
