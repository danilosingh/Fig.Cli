namespace Fig.Cli.Helpers
{
    public static class StringHelper
    {
        public static string CompleteWithEmptySpaces(this string str, int length)
        {
            return str.PadRight(length, ' ');
        }
    }
}
