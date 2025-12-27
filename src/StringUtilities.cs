namespace MyShell.Core
{
    public static class StringUtilities
    {
        public static string GetLongestCommonPrefix(List<string> strings)
        {
            if (strings == null || strings.Count == 0)
                return string.Empty;

            if (strings.Count == 1)
                return strings[0];

            var sortedStrings = strings.OrderBy(s => s).ToList();
            var first = sortedStrings.First();
            var last = sortedStrings.Last();

            int commonLength = 0;
            int maxLength = Math.Min(first.Length, last.Length);

            while (commonLength < maxLength && first[commonLength] == last[commonLength])
            {
                commonLength++;
            }

            return first[..commonLength];
        }
    }
}
