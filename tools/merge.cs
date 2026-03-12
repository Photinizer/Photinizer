using System;
using System.IO;
using System.Linq;
using System.Text;

string[] extensions = args.Length > 1 ? args[1].Split(';') : [".csproj", ".props", ".targets", ".js", ".cs"];
string[] exclude = ["\\bin\\", "\\obj\\"];

var currentDir = args.Length > 0 ? Path.GetFullPath(args[0]) : AppContext.BaseDirectory;
if (!Directory.Exists(currentDir))
{
    Console.WriteLine($"Directory does not exist: {currentDir}");
    return;
}

var contextName = Path.GetFileName(currentDir);

if (!currentDir.EndsWith(Path.DirectorySeparatorChar))
{
    currentDir += Path.DirectorySeparatorChar;
}

Console.WriteLine($"Directory: {currentDir}, Context: {contextName}");

var enabledExts = extensions.Where(ext => !ext.StartsWith('-')).ToList();
var files = Directory.GetFiles(currentDir, "*", SearchOption.AllDirectories).Where(f => !exclude.Any(ex => f.Contains(ex)) && enabledExts.Contains(Path.GetExtension(f))).ToList();

if (files.Count == 0)
{
    Console.WriteLine($"Nothing to merge.");
    return;
}

var extensionOrder = enabledExts.Select((ext, index) => new { ext, index }).ToDictionary(x => x.ext, x => x.index);
var sortedFiles = files.OrderBy(f => extensionOrder[Path.GetExtension(f)]).ThenBy(f => f).ToList();

var fileNames = string.Join("; ", sortedFiles.Select(f => f.Substring(currentDir.Length)));
Console.WriteLine($"Files[{sortedFiles.Count}]: {fileNames}");

string outputPath = Path.Combine(Environment.CurrentDirectory, $"{contextName}-merged.txt");

using (var writer = new StreamWriter(outputPath, false, Encoding.UTF8))
{
    writer.WriteLine($"// ----- Begin Project {contextName} -----");

    foreach (var file in sortedFiles)
    {
        string relativePath = file.Substring(currentDir.Length);
        writer.WriteLine($"// ----- Begin file: {relativePath} -----");

        string content = File.ReadAllText(file);
        writer.Write(content);
        writer.WriteLine();

        writer.WriteLine($"// ------ End file {relativePath} ------");
        writer.WriteLine();
    }

    writer.WriteLine($"// ------ End Project {contextName} ------");
}

Console.WriteLine($"Merged content written to {outputPath}");