using System;
using System.Collections.Generic;
using System.Text;

namespace Pulse.FS
{
    public sealed class SteamKeyValues
    {
        private readonly List<SteamKeyValues> _children = new();

        public string Name { get; }
        public string Value { get; }
        public IReadOnlyList<SteamKeyValues> Children => _children;
        public bool IsBlock => _children.Count > 0;

        private SteamKeyValues(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public static SteamKeyValues Parse(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            int position = 0;
            return ParseBlock(text, ref position, null);
        }

        public SteamKeyValues Child(string name)
        {
            foreach (SteamKeyValues child in _children)
            {
                if (string.Equals(child.Name, name, StringComparison.OrdinalIgnoreCase))
                    return child;
            }

            return null;
        }

        public IEnumerable<SteamKeyValues> ChildrenNamed(string name)
        {
            foreach (SteamKeyValues child in _children)
            {
                if (string.Equals(child.Name, name, StringComparison.OrdinalIgnoreCase))
                    yield return child;
            }
        }

        public string GetValue(string name)
        {
            SteamKeyValues child = Child(name);
            if (child == null || child.IsBlock)
                return null;

            return child.Value;
        }

        public IEnumerable<SteamKeyValues> Blocks()
        {
            foreach (SteamKeyValues child in _children)
            {
                if (child.IsBlock)
                    yield return child;
            }
        }

        private static SteamKeyValues ParseBlock(string text, ref int position, string name)
        {
            SteamKeyValues result = new(name, null);

            while (true)
            {
                SkipWhitespace(text, ref position);

                if (position >= text.Length)
                    break;

                if (text[position] == '}')
                {
                    position++;
                    break;
                }

                string key = ReadToken(text, ref position);
                if (key == null)
                {
                    position++;
                    continue;
                }

                SkipWhitespace(text, ref position);

                if (position < text.Length && text[position] == '{')
                {
                    position++;
                    result._children.Add(ParseBlock(text, ref position, key));
                    continue;
                }

                string value = null;
                if (position < text.Length && text[position] != '}')
                    value = ReadToken(text, ref position);

                result._children.Add(new(key, value ?? string.Empty));
            }

            return result;
        }

        private static void SkipWhitespace(string text, ref int position)
        {
            while (position < text.Length)
            {
                char symbol = text[position];

                if (char.IsWhiteSpace(symbol))
                {
                    position++;
                    continue;
                }

                if (symbol == '/' && position + 1 < text.Length && text[position + 1] == '/')
                {
                    while (position < text.Length && text[position] != '\n')
                        position++;

                    continue;
                }

                break;
            }
        }

        private static string ReadToken(string text, ref int position)
        {
            if (position >= text.Length)
                return null;

            if (text[position] == '"')
                return ReadQuoted(text, ref position);

            int start = position;

            while (position < text.Length)
            {
                char symbol = text[position];

                if (char.IsWhiteSpace(symbol) || symbol == '{' || symbol == '}' || symbol == '"')
                    break;

                position++;
            }

            if (position == start)
                return null;

            return text.Substring(start, position - start);
        }

        private static string ReadQuoted(string text, ref int position)
        {
            position++;

            StringBuilder builder = new();

            while (position < text.Length)
            {
                char symbol = text[position++];

                if (symbol == '"')
                    return builder.ToString();

                if (symbol != '\\')
                {
                    builder.Append(symbol);
                    continue;
                }

                if (position >= text.Length)
                    break;

                char escaped = text[position++];

                switch (escaped)
                {
                    case 'n':
                        builder.Append('\n');
                        break;
                    case 't':
                        builder.Append('\t');
                        break;
                    case '\\':
                        builder.Append('\\');
                        break;
                    case '"':
                        builder.Append('"');
                        break;
                    default:
                        builder.Append(escaped);
                        break;
                }
            }

            return builder.ToString();
        }
    }
}
