using System;
using Grooper;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Security.Policy;
using System.Diagnostics;

namespace GrooperGit
{
    /// <summary>
    /// Git Repository
    /// </summary>
    [DataContract, HelpBase]
    public class GitRepository : EmbeddedObject
    {


        private string _LocalPath = Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), "GrooperGit", Guid.NewGuid().ToString());


        /// <inheritdoc/>
        public GitRepository(ConnectedObject Owner) : base(Owner)
        {

        }

        /// <summary>Represents the location of the local Git repository</summary>
        /// <remarks>The path is also where the required Git CLI runs</remarks>
        [DataMember, Viewable, DisplayName("Local Path")]
        public string LocalPath
        {
            get { return _LocalPath; }
            
            set 
            {   
                var directoryInfo = new DirectoryInfo(LocalPath);
                if (!directoryInfo.Exists) directoryInfo.Create();
                  
                _LocalPath = value; 
                //instance.OwnerNode.PropsDirty = true;   
            }
        }

        /// <summary>Represents the local path of the Git repository associated with the Grooper Project.</summary>
        /// <remarks>The repository URL points to the location where the Git project is hosted. Developers can use this URL to clone, fetch, or push changes to the repository. It's an essential link for collaboration and code management.</remarks>
        [DataMember, Viewable, DisplayName("Local Repository"), Required, Category("Local")]
        public string LocalRepository { get; set; }

        /// <summary>Manages Git branches within the specified GitProject.</summary>
        /// <remarks>Git branches allow developers to work on features or fixes in isolation, without affecting the main or other development lines. This ensures code stability and streamlined collaboration. More details can be found <a href="https://git-scm.com/docs/git-branch">here</a>.</remarks>
        [DataMember, Viewable, DisplayName("Local Branch"), TypeConverter(typeof(BranchListConverter)), Category("Local")]
        public string LocalBranch { get; set; }

        /// <summary>Represents the remote path of the Git repository associated with the Grooper Project.</summary>
        /// <remarks>The repository URL points to the location where the Git project is hosted. Developers can use this URL to clone, fetch, or push changes to the repository. It's an essential link for collaboration and code management.</remarks>
        [DataMember, Viewable, DisplayName("Remo Repository"), Required, Category("Remote")]
        public string RemoteRepository { get; set; }

        /// <summary>Manages Git branches within the specified GitProject.</summary>
        /// <remarks>Git branches allow developers to work on features or fixes in isolation, without affecting the main or other development lines. This ensures code stability and streamlined collaboration. More details can be found <a href="https://git-scm.com/docs/git-branch">here</a>.</remarks>
        [DataMember, Viewable, DisplayName("Local Branch"), TypeConverter(typeof(BranchListConverter)), Category("Remote")]
        public string RemoteBranch { get; set; }

        #region Methods
        public void Init()
        {
            BaseCommand();
        }

        public void Add(string fileName)
        {
            BaseCommand($"add '{fileName}'");
        }


        public void Remove(string fileName)
        {
            BaseCommand($"remove '{fileName}'");
        }

        public void Commit(string commitMesage)
        {
            BaseCommand($"commit -m '{commitMesage}'");
        }

        public void Push(string branch, string remote, string arguments="")
        {
            BaseCommand($"push '{branch}' '{remote}' {arguments}");
        }
        #endregion

        /// <summary>
        /// Command to be executed by the windows server powershell the command is already prefixed with 'git'
        /// </summary>
        /// <param name="gitArguments">example command -m 'Patched Issue'</param>
        /// <returns>returns the output of the console as string</returns
        private string BaseCommand(string gitArguments = "help")
        {
            var di = new DirectoryInfo(LocalPath);
            if (!di.Exists) di.Create();
            string command = $"cd '{LocalPath}'; git {gitArguments}";

            var process = new Process
            {

                StartInfo = new ProcessStartInfo
                {
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
                // Assuming GrooperException is a custom exception you have defined.
                throw new Exception($"GitOutput: {errorOutput}");
            }

            return output;
        }
        /// <inheritdoc/>
        public override ValidationErrorList ValidateProperties()
        {
            return base.ValidateProperties();
        }
    }
}
