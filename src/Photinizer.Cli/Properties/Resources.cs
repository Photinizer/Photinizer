namespace Photinizer.Cli.Properties;

internal static class Resources
{
    private static readonly string[] s_lines =
    [
        "There are 10 types of people: those who understand binary and those who don't.",
        "It's not a bug; it's an undocumented feature.",
        "I would tell you a UDP joke, but you might not get it.",
        "To understand recursion, you must first understand recursion.",
        "The three hard problems in CS: cache invalidation, naming things, and off-by-one errors.",
        "Regex: now you have two problems.",
        "Debugging: being the detective in a crime you committed.",
        "Works on my machine. Must be hardware acceleration.",
        "My code has no bugs. It just develops random features.",
        "In theory, theory and practice are the same. In practice, they are not.",
        "640K ought to be enough for anybody. (Narrator: it was not.)",
        "0xDEADBEEF is a balanced breakfast.",
        "Premature optimization is the root of all evil. (But it’s fun.)",
        "I’d explain pointers, but you’d get null out of it.",
        "Ship it. We’ll add tests in production.",
        "A SQL query walks into a bar and orders a table.",
        "rm -rf /problems (access denied).",
        "Undefined behavior: Schrödinger's bug.",
        "Git commit -m \"Fix stuff\". Future me: Which stuff?",
        "Never attribute to malice what can be explained by race conditions.",
        "Programmer: a machine that turns coffee into stack traces.",
        "Bold of you to assume it compiles.",
        "It compiled. Run.",
        "It ran. Panic.",
        "If it compiles, ship it. If it runs, benchmark it.",
        "99 little bugs in the code, take one down, patch it around, 127 little bugs in the code.",
        "My favorite IDE feature is the red squiggly under my life choices.",
        "I don’t always test my code, but when I do, I test in prod.",
        "Segmentation fault: core dumped (my hopes too).",
        "Sudo make me a sandwich. Okay.",
        "“Why is it slow?” — It depends.",
        "0, 1, many.",
        "Out of memory is just the GC asking for a raise.",
        "The cloud is just someone else’s computer.",
        "If it hurts, automate it. If automation hurts, script it more.",
        "Containerized my app; now the bugs are portable.",
        "Asynchronous: now with twice the race conditions.",
        "Monolith or microservices? Yes.",
        "If you see me smiling, I just found a Heisenbug.",
        "Unit tests are like seatbelts: annoying until you crash.",
        "Performance tip: don’t do the slow thing.",
        "Latency is the new downtime.",
        "The spec is a Schrödinger document: both final and draft.",
        "YAGNI until you really, really need it.",
        "Temporary fix: permanent edition.",
        "Comment your code. Future you is a different person.",
        "Feature flag: the adult version of if-else.",
        "I refactored it so well I no longer understand it.",
        "CRLF vs LF: choose your battles.",
        "Merge conflict: collaborative art."
    ];

    private static readonly Random s_rng = new();
    internal static bool ShouldGenerate() => s_rng.Next(100) % 2 == 0;
    internal static string Generate() => s_lines[s_rng.Next(s_lines.Length)];
}