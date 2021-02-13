using System.Diagnostics;

namespace Fig.Cli.Helpers
{
    public static class BrowserHelper
    {
        public static void OpenUrl(string url)
        {
            try
            {
                Process.Start("chrome", url);
            }
            catch
            {
                Process.Start(url);
            }
        }
    }
}
