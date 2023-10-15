using System;
using System.Diagnostics;
using System.IO;

namespace GrooperGit
{
    /// <summary>
    /// An abstract for running CLI commmands through powershell. 
    /// </summary>
    public class Shell
    {
        ///<summary>The current working directory of the shell</summary>
        public string WorkingDirectory { get; }

        ///<summary>boolean showing whethe or not the console returned an error</summary> 
        public bool Error { get; private set; }

        ///<summary>boolean showing whethe or not the console returned an error</summary> 
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// Contsctructor
        /// </summary>
        /// <param name="cwd">The current working directory of the shell</param>
        public Shell(string cwd)
        {
            WorkingDirectory = cwd;
        }

        ///<summary>Runs a powershell command.</summary>
        ///<remarks>Shell is disposed between calls and will not store any scrollback.</remarks>
        ///<param name="application">The application to be ran i.e ping, mkdir, git.</param>
        ///<param name="args">The arguments passed into the clie</param>
        public string Command(string application, string args)
        {
            DirectoryInfo di = new DirectoryInfo(WorkingDirectory);
            if (!di.Exists)
            {
                di.Create();
            }

            string command = $"{application} {args}";

            Process process = new Process
            {

                StartInfo = new ProcessStartInfo
                {
                    WorkingDirectory = WorkingDirectory,
                    FileName = "powershell.exe",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command {command}"

                }
            };

            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            string errorOutput = process.StandardError.ReadToEnd();
            process.WaitForExit();
            if (!string.IsNullOrEmpty(errorOutput))
            {
                Error = true;
                ErrorMessage = errorOutput;
                throw new Exception($"GitOutput: {errorOutput}");

            }
            ErrorMessage = "";
            Error = false;
            return output;
        }
    }
}
