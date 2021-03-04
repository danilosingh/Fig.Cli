using System;

namespace Fig.Cli.Extensions
{
    public static class StringExtensions
    {
        public static string[] Split(this string str, string separator, StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries, int count = 0)
        {
            return count > 0 ?
                str.Split(new string[] { separator }, count, options) :
                str.Split(new string[] { separator }, options);
        }

        public static string Replace(this string str, string[] strings, string newString)
        {
            foreach (var item in strings)
            {
                str = str.Replace(item, newString);
            }

            return str;
        }
    }
}
