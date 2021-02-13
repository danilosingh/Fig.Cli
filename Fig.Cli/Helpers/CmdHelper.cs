using System;

namespace Fig.Cli.Helpers
{
    public static class CmdHelper
    {
        public static CmdResult ExecuteCommand(string command, string workingDirectory = null, bool writeCommand = true,
            bool writeOutput = true, bool useCommandIndicator = true)
        {
            if (writeCommand)
                Console.WriteLine((useCommandIndicator ? "> " : "") + command);

            System.Diagnostics.ProcessStartInfo procStartInfo =
                new System.Diagnostics.ProcessStartInfo("cmd", "/c " + command);

            if (!string.IsNullOrEmpty(workingDirectory))
                procStartInfo.WorkingDirectory = workingDirectory;

            procStartInfo.RedirectStandardError = true;
            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.UseShellExecute = false;
            procStartInfo.CreateNoWindow = true;
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo = procStartInfo;
            proc.Start();
            proc.WaitForExit();
            string output = proc.StandardOutput.ReadToEnd() + proc.StandardError.ReadToEnd();

            if (writeOutput)
                Console.Write(output);

            return new CmdResult() { Output = output, ExitCode = proc.ExitCode };
        }
    }

    public class CmdResult
    {
        public string Output { get; set; }
        public int ExitCode { get; set; }
        public bool IsSuccess { get { return ExitCode == 0; } }
    }
}
