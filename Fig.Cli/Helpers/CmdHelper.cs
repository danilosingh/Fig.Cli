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

            // Cross-platform: Windows usa cmd /c; macOS/Linux usa /bin/sh -c.
            System.Diagnostics.ProcessStartInfo procStartInfo;
            if (OperatingSystem.IsWindows())
            {
                procStartInfo = new System.Diagnostics.ProcessStartInfo("cmd", "/c " + command);
            }
            else
            {
                procStartInfo = new System.Diagnostics.ProcessStartInfo("/bin/sh");
                procStartInfo.ArgumentList.Add("-c");
                procStartInfo.ArgumentList.Add(command);
            }

            if (!string.IsNullOrEmpty(workingDirectory))
                procStartInfo.WorkingDirectory = workingDirectory;

            procStartInfo.RedirectStandardError = true;
            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.UseShellExecute = false;
            procStartInfo.CreateNoWindow = true;
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo = procStartInfo;
            proc.Start();
            string output = proc.StandardOutput.ReadToEnd() + proc.StandardError.ReadToEnd();

            // ReadToEnd() retornar (EOF dos pipes) NAO garante que o processo encerrou;
            // sem WaitForExit, ler proc.ExitCode pode disparar "Process must exit before
            // requested information can be determined" em comandos rapidos (race).
            proc.WaitForExit();

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
