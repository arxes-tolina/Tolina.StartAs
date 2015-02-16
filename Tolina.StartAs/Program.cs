using CommandLine;
using CommandLine.Text;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Security;

namespace Tolina.StartAs
{

    class Options
    {
        [Option('u', "user", Required = true, HelpText = "Useraccount to run under")]
        public string UserName { get; set; }

        [Option('p', "password", Required = true, HelpText = "Password of the useraccount")]
        public string Password { get; set; }

        [Option('e', "executable", Required = true, HelpText = "Path to executable")]
        public string Executable { get; set; }

        [Option('a', "arguments", Required = false, HelpText = "Additional arguments")]
        public string Arguments { get; set; }

        [Option('w', "workdir", Required = true, HelpText = "Working directory")]
        public string Workdir { get; set; }

        [Option('v', "verbose", Required = false, DefaultValue = false, HelpText = "Prints all messages to standard output.")]
        public bool Verbose { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }

    class Program
    {
        private static SecureString GetSecureString(string str)
        {
            var secureString = new SecureString();
            foreach (char ch in str)
            {
                secureString.AppendChar(ch);
            }
            secureString.MakeReadOnly();
            return secureString;
        }

        private static void StandardOutputReceiver(object sendingProcess, DataReceivedEventArgs outLine)
        {
            // Receives the child process' standard output
            if (!string.IsNullOrEmpty(outLine.Data))
            {
                Console.WriteLine(outLine.Data);
            }
        }

        private static void StandardErrorReceiver(object sendingProcess, DataReceivedEventArgs errLine)
        {
            // Receives the child process' standard error
            if (!string.IsNullOrEmpty(errLine.Data))
            {
               Console.Error.WriteLine(errLine.Data);
            }
        }

        private const string Homedrive = "C:";
        private const string Roaming = "\\AppData\\Roaming";
        private const string Local = "\\AppData\\Local";

        static void Main(string[] args)
        {

            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                String[] userAndDomain=options.UserName.Split('\\');
                String username;
                String domain = "";
                if (userAndDomain.Length == 2)
                { 
                    username = userAndDomain[1];
                    domain = userAndDomain[0];
                }
                else
                {
                    username = userAndDomain[0];
                }
                String password = options.Password;
                String executable = options.Executable;
                String workdir = options.Workdir;
                String parameters = options.Arguments;

                if (options.Verbose)
                {
                    Console.WriteLine("User domain:          {0}", domain);
                    Console.WriteLine("User account:         {0}", username);
                    Console.WriteLine("Password:             {0}", options.Password);
                    Console.WriteLine("Working directory:    {0}", options.Workdir);
                    Console.WriteLine("Path to executable:   {0}", options.Executable);
                    Console.WriteLine("Additional arguments: {0}", options.Arguments);
                }

                try
                {
                    // set up process to start as specified user
                    var info = new ProcessStartInfo();
                    info.CreateNoWindow = true;
                    info.RedirectStandardOutput = true;
                    info.RedirectStandardError=true;
                    info.FileName = executable;
                    info.WorkingDirectory = workdir;
                    info.UserName = username;
                    info.Domain = domain;
                    info.Password = GetSecureString(password);
                    info.Arguments = parameters;
                    info.LoadUserProfile = true;
                    info.UseShellExecute = false;
                    
                    // since we have to modify Environment, the target-user's environment variables are overwritten,
                    // whyever Windows does that.
                    // So we set the relevant variables anew, which makes some assumptions.
                    // This is valid and tested at least with Windows 8 default installation

                    
                    String homepath = "\\Users\\"+username;
                    String userprofile = Homedrive + homepath;

                    info.EnvironmentVariables["APPDATA"] = userprofile + Roaming;
                    info.EnvironmentVariables["HOMEDRIVE"] = Homedrive;
                    info.EnvironmentVariables["HOMEPATH"] = homepath;
                    info.EnvironmentVariables["LOCALAPPDATA"] = userprofile + Local;
                    info.EnvironmentVariables["USERDOMAIN"] = domain;
                    info.EnvironmentVariables["USERNAME"] = username;
                    info.EnvironmentVariables["USERPROFILE"] = userprofile;

                    // create process
                    var process = new Process();
                    process.StartInfo = info;
                   
                    // capture output
                    process.OutputDataReceived += new DataReceivedEventHandler(StandardOutputReceiver);
                    process.ErrorDataReceived += new DataReceivedEventHandler(StandardErrorReceiver);

                    //Execute the process
                    process.Start();
                    int pid = process.Id;
                    if (options.Verbose)
                    {
                        Console.WriteLine("--- Start output of process {0} ---", pid);
                    }
                    
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();

                    if (options.Verbose)
                    {
                        Console.WriteLine("--- End output of process {0} ---", pid);
                        Console.WriteLine("Process {0} exited with code {1}", pid, process.ExitCode);
                    }
                    
                    Environment.Exit(process.ExitCode);
                } 
                catch (Win32Exception e) 
                {
                    Console.Error.WriteLine(e.Message);
                    Environment.Exit(1);
                }
            }
        }
    }
}
