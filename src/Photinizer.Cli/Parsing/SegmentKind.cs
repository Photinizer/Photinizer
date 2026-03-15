namespace Photinizer.Cli.Parsing;

public enum SegmentKind
{
    Text = 0,
    SquarePlaceholder = 1, // [[...]]
    CurlyPlaceholder = 2   // {{...}}
}
