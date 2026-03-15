using Photinizer.Cli.Parsing;

namespace Photinizer.Cli.Services;

public readonly ref struct PlaceholderProvider(Dictionary<string, string> square, Dictionary<string, string> curly)
{
    public bool TryGetValue(SegmentKind kind, string placeholder, out string? value)
    {
        value = null;
        return kind switch
        {
            SegmentKind.SquarePlaceholder => square.TryGetValue(placeholder, out value),
            SegmentKind.CurlyPlaceholder => curly.TryGetValue(placeholder, out value),
            _ => false
        };
    }
}

