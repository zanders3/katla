using System;
using System.Diagnostics;

namespace zanders3.Katla
{
    public static class ProcessHelper
    {
        public static bool Run(Action<string> logMessage, string command, params string[] arguments)
        {
            logMessage(command + " " + String.Join(" ", arguments));
            Process process = Process.Start(new ProcessStartInfo()
            {
                FileName = command,
                Arguments = String.Join(" ", arguments),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            });
            process.EnableRaisingEvents = true;
            process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) => logMessage(e.Data);
            process.OutputDataReceived += (object sender, DataReceivedEventArgs e) => logMessage(e.Data);

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            return process.ExitCode == 0;
        }
    }
}

