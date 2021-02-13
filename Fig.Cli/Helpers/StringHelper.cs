namespace Fig.Cli.Helpers
{
    public static class StringHelper
    {
        public static string ConcatPath(params string[] paths)
        {
            string path = string.Empty;
            foreach (var item in paths)
            {
                path += ConcatSlash(item);
            }
            return path.Remove(path.Length-1);
        }

        private static string ConcatSlash(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            if (path[path.Length - 1] != '\\')
                path += @"\";

            return path;
        }

        public static string CompleteWithEmptySpaces(this string str, int length)
        {
            return str.PadRight(length, ' ');
        }
    }
}
