using System.Text.RegularExpressions;

namespace SpamBlocker.Utils
{
    public static class RegexUtils
    {
        public static readonly Regex UrlRegex = new Regex(
            @"(?:(?:https?|ftp|steam):\/\/)?(?:www\.)?(?:[a-zA-Z0-9](?:[a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}\b(?:[-a-zA-Z0-9()@:%_\+.~#?&=\/]*)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        public static readonly Regex IpRegex = new Regex(
            @"\b(?:(?:25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\.){3}(?:25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\b",
            RegexOptions.Compiled
        );

        public static readonly Regex IpPortRegex = new Regex(
            @"\b(?:(?:25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\.){3}(?:25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9]):(?:[1-9][0-9]{0,4}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])\b",
            RegexOptions.Compiled
        );

        public static bool IsWholeWordMatch(string content, string word, bool caseSensitive)
        {
            var pattern = $@"\b{Regex.Escape(word)}\b";
            var options = caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
            return Regex.IsMatch(content, pattern, options);
        }

        public static bool ContainsWord(string content, string word, bool caseSensitive)
        {
            var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            return content.Contains(word, comparison);
        }
    }
}