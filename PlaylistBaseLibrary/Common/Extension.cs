namespace System
{
    using System.Globalization;
    using System.Text;
    using System.Text.RegularExpressions;
    public class Common
    {
        public static T2 TryGet<T, T2>(Func<T, T2> func, T param)
        {
            try
            {
                return func(param);
            }
            catch (Exception) { }

            return default;
        }
    }

    public static class Extension
    {
        /// <summary>
        /// Unix TimeStamp to DateTime
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static DateTime UnixTimeStampToDateTime(this string text)
        {
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            if (string.IsNullOrEmpty(text))
                return dtDateTime;
            dtDateTime = dtDateTime.AddSeconds(Convert.ToInt32(text));
            return dtDateTime;
        }

        /// <summary>
        /// Converts a given DateTime into a Unix timestamp
        /// </summary>
        /// <param name="value">Any DateTime</param>
        /// <returns>The given DateTime in Unix timestamp format</returns>
        public static int ToUnixTimestamp(this DateTime value)
        {
            return (int)Math.Truncate((value.ToUniversalTime().Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
        }

        /// <summary>
        /// Gets a Unix timestamp representing the current moment
        /// </summary>
        /// <param name="ignored">Parameter ignored</param>
        /// <returns>Now expressed as a Unix timestamp</returns>
        public static int UnixTimestamp(this DateTime ignored)
        {
            return (int)Math.Truncate((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
        }

        public static string RemoveChars(this string me, char[] chars)
        {
            foreach (var c in chars)
            {
                var index = me.IndexOf(c);
                if (index > 0)
                    me = me.Remove(index, 1);
            }
            return me;
        }

        public static string RemoveStrings(this string me, params string[] strings)
        {
            var c = string.Join("|", strings);
            me = Regex.Replace(me, $@"\b({c})\b|(\b0?(?<chif>[0-9_]+))({c})\b|\b({c})0?(?<chif>[0-9_]+\b)", "${chif}", RegexOptions.IgnoreCase);
            return Regex.Replace(me, @"\s+", " ").Trim();
        }

        public static string RemoveRegex(this string me, params string[] regexs)
        {
            foreach (var regex in regexs)
                me = Regex.Replace(me, $"{regex}", string.Empty, RegexOptions.IgnoreCase);
            return me;
        }

        public static string RemoveDiacritics(this string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    stringBuilder.Append(c);
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        /// <summary>
        /// Scoring two strings
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static int LevenshteinDistance(this string source, string target)
        {
            if (String.IsNullOrEmpty(source))
            {
                if (String.IsNullOrEmpty(target)) return 0;
                return target.Length;
            }
            if (String.IsNullOrEmpty(target)) return source.Length;

            if (source.Length > target.Length)
            {
                var temp = target;
                target = source;
                source = temp;
            }

            var m = target.Length;
            var n = source.Length;
            var distance = new int[2, m + 1];
            // Initialize the distance 'matrix'
            for (var j = 1; j <= m; j++) distance[0, j] = j;

            var currentRow = 0;
            for (var i = 1; i <= n; ++i)
            {
                currentRow = i & 1;
                distance[currentRow, 0] = i;
                var previousRow = currentRow ^ 1;
                for (var j = 1; j <= m; j++)
                {
                    var cost = (target[j - 1] == source[i - 1] ? 0 : 1);
                    distance[currentRow, j] = Math.Min(Math.Min(
                                distance[previousRow, j] + 1,
                                distance[currentRow, j - 1] + 1),
                                distance[previousRow, j - 1] + cost);
                }
            }
            return distance[currentRow, m];
        }

        /// <summary>
        /// Remove all whitespaces
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string FullTrim(this string text)
            => Regex.Replace(text, @"\s+", "");

      
    }
}
