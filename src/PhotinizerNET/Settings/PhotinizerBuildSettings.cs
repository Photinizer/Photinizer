using System.Text.RegularExpressions;

namespace PhotinizerNET.Core.Settings;

public class PhotinizerBuildSettings
{
    private string _args;

    private const string _buildSouceArg = "--build-source";
    private string _buildSource;

    public PhotinizerBuildSettings()
    {
        _args = string.Join(" ", Environment.GetCommandLineArgs());
        IsBuildMode = _args.Contains(_buildSouceArg);
    }

    public bool IsBuildMode { get; private set; }

    public string BuildSource => _buildSource ??= GetBuildSource();

    private string GetBuildSource()
        => Regex.Match(_args, "--build-source=\"(.+?)\"").Groups[1].Value;
}
