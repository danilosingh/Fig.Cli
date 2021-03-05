using System;

namespace Fig.Cli
{
    public static class CommandFactory
    {
        public static CommandResult Execute<T>(BaseOptions opts)
            where T : Command
        {
            var toolVersion = typeof(CommandFactory).Assembly.GetName().Version;
            var toolPlatform = Environment.OSVersion.Platform == PlatformID.Win32NT ? "windows" : "linux";
            
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine($"Running fig v{toolVersion.Major}.{toolVersion.Minor}.{toolVersion.Build} for {toolPlatform}-x64");
            Console.WriteLine($"Visit https://github.com/danilosingh/Fig.Cli for documentation & more samples{Environment.NewLine}");
            Console.ResetColor();

            return ((T)Activator.CreateInstance(typeof(T), opts, FigContext.Instance)).Execute();
        }
    }
}
