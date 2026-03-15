namespace Photinizer.Cli.Parsing
{
    public readonly ref struct Parser(string content)
    {
        public List<(SegmentKind Kind, ReadOnlyMemory<char> Name, ReadOnlyMemory<char> Slice)> Parse()
        {
            if (string.IsNullOrEmpty(content))
            {
                return [];
            }

            var mem = content.AsMemory();
            var span = content.AsSpan();

            var list = new List<(SegmentKind, ReadOnlyMemory<char>, ReadOnlyMemory<char>)>(Math.Max(4, span.Length / 64));

            int i = 0;
            int textStart = 0;
            int length = span.Length;

            while (i < length)
            {
                if (!IsOpenPair(span, i, out char closer, out var openKind))
                {
                    i++;
                    continue;
                }

                // We have an opening pair at i ("[[" or "{{").
                int openStart = i;
                int j = i + 2;
                bool progressed = false;

                while (j < length - 1)
                {
                    // Rule: if we see a *new* opening before the matching close,
                    // dump everything up to this new opening as plain text and restart from there.
                    if (IsOpenPair(span, j, out _, out _))
                    {
                        if (j > textStart)
                        {
                            list.Add((SegmentKind.Text, ReadOnlyMemory<char>.Empty, mem.Slice(textStart, j - textStart)));
                        }

                        i = j;                // restart from newer opening
                        progressed = true;
                        break;
                    }

                    // Matching close found?
                    if (span[j] == closer && span[j + 1] == closer)
                    {
                        // Emit plain text before the placeholder
                        if (openStart > textStart)
                        {
                            list.Add((SegmentKind.Text, ReadOnlyMemory<char>.Empty, mem.Slice(textStart, openStart - textStart)));
                        }

                        // Emit placeholder including its brackets
                        int len = (j + 2) - openStart;
                        list.Add((openKind, mem.Slice(openStart + 2, len - 4), mem.Slice(openStart, len)));

                        i = j + 2;
                        textStart = i;
                        progressed = true;
                        break;
                    }

                    j++;
                }

                if (!progressed)
                {
                    // No closing pair till end → treat the rest as plain text
                    break;
                }
            }

            if (textStart < length)
            {
                list.Add((SegmentKind.Text, ReadOnlyMemory<char>.Empty, mem.Slice(textStart, length - textStart)));
            }

            return list;
        }

        private static bool IsOpenPair(scoped ReadOnlySpan<char> s, int pos, out char closer, out SegmentKind kind)
        {
            closer = '\0';
            kind = SegmentKind.Text;
            if (pos + 1 >= s.Length) return false;

            switch (s[pos])
            {
                case '[' when s[pos + 1] == '[':
                    closer = ']';
                    kind = SegmentKind.SquarePlaceholder;
                    return true;
                case '{' when s[pos + 1] == '{':
                    closer = '}';
                    kind = SegmentKind.CurlyPlaceholder;
                    return true;
                default:
                    return false;
            }
        }

    }
}
