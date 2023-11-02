using System;
using Grooper;
using System.IO;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GrooperGit
{
    /// <summary>
    /// Git Repository
    /// </summary>
    [DataContract, HelpBase]
    public class GitRepository : EmbeddedObject
    {
        private string _localPath;

        /// <inheritdoc/>
        public GitRepository(ConnectedObject Owner) : base(Owner)
        {
            GitProject projectNode = (GitProject)Database.GetNode(OwnerNode.Id);
            _localPath = projectNode.LocalPath;
        }

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

        /// <summary>Creates the necessary files for all nodes in the local git repository.</summary>
        /// <remarks>This will create each node as a folder in the root of the local path, and all settings, and files in that folder.</remarks>
        public void SyncFiles()
        {
            foreach (GrooperNode node in OwnerNode.AllChildren)
            {
                NodeAsFile nodeAsFile = new NodeAsFile(node);
                nodeAsFile.WriteAll();
            }
        }

        ///<summary>test</summary>
        public void AddRemote()
        {
            _ = BaseCommand($"git remote add ");
        }
        /// <summary>Executes a Git Compare/Diff operation, showing differences between files or branches in the specified GitProject [Object Command].</summary>
        /// <remarks>A Git Compare/Diff highlights the changes between two sets of code, aiding in understanding modifications and aiding in code review. More details can be found <a href="https://git-scm.com/docs/git-diff">here</a>.</remarks>
        public IEnumerable<string> Diff(GrooperNode node)
        {
            string path = Path.Combine(_localPath, node.Id.ToString());
            return BaseCommand($"diff {path}").Split('\n');
        }
        ///<summary>
        ///Initializes a new Git repository for the given repository name.
        ///This method invokes the 'git init' command which initializes a new Git repository and begins tracking an existing directory.
        ///For more information, refer to the official Git documentation: https://git-scm.com/docs/git-init
        ///</summary>
        public void Init()
        {
            _ = BaseCommand($"init ");
        }

        ///<summary>
        ///Stages the specified files or directories in preparation for a commit.
        ///The 'git add' command updates the index using the current content found in the working tree.
        ///For more information, refer to the official Git documentation: https://git-scm.com/docs/git-add
        ///</summary>
        public void Add(string fileName)
        {
            _ = BaseCommand($"add '{fileName}'");
        }

        ///<summary>
        ///Unstages or removes the specified files or directories from the index and the working tree.
        ///The 'git rm' command removes files from the working tree and from the index.
        ///For more information, refer to the official Git documentation: https://git-scm.com/docs/git-rm
        ///</summary>
        public void Remove(GrooperNode Item)
        {
            _ = BaseCommand($"reset /{Item.Id}");
        }
        ///<summary>Executes a Git Commit operation on the specified GitProject [Object Command].</summary>
        ///<remarks>A Git Commit captures changes to files in the project, allowing for tracking of modifications and collaboration with other contributors. More details can be found <a href="https://git-scm.com/docs/git-commit">here</a>.</remarks>
        public void Commit(string commitMesage)
        {
            _ = BaseCommand($"commit -m '{commitMesage}'");
        }

        /// <summary>Executes a Git Push operation, sending local changes from the specified GitProject to a remote repository [Object Command].</summary>
        /// <remarks>A Git Push updates the remote repository with commits made locally, sharing modifications with other contributors. More details can be found <a href="https://git-scm.com/docs/git-push">here</a>.</remarks>
        public void Push(string branch, string remote, string arguments = "")
        {
            _ = BaseCommand($"push '{branch}' '{remote}' {arguments}");
        }

        ///<summary>
        ///Interacts with the branch command in Git.
        ///The 'git branch' command lists, creates, or deletes branches.
        ///For more information, refer to the official Git documentation: https://git-scm.com/docs/git-branch
        ///</summary>
        ///<param name="operation">The operation to perform: "list", "create", or "delete".</param>
        ///<param name="branchName">The name of the branch, if applicable.</param>
        public string Branch(string operation, string branchName = "")
        {
            switch (operation.ToLower())
            {
                case "list":
                    return BaseCommand("branch");
                case "create":
                    return !string.IsNullOrEmpty(branchName)
                        ? BaseCommand("branch {branchName}")
                        : throw new ArgumentException("Branch name is required for create operation.");
                case "delete":
                    return !string.IsNullOrEmpty(branchName)
                        ? BaseCommand("branch -d {branchName}")
                        : throw new ArgumentException("Branch name is required for delete operation.");
                default:
                    throw new ArgumentException($"Invalid operation: {operation}. Supported operations are 'list', 'create', and 'delete'.");
            }
        }
        #endregion

        /// <summary>
        /// Command to be executed by the windows server powershell the command is already prefixed with 'git'
        /// </summary>
        /// <param name="gitArguments">example command -m "Patched Issue"</param>
        /// <returns>returns the output of the console as string</returns>
        private string BaseCommand(string gitArguments = "help")
        {
            Shell shell = new Shell(_localPath);
            return shell.Command("git", gitArguments);
        }

        /// <inheritdoc/>
        public override ValidationErrorList ValidateProperties()
        {
            return base.ValidateProperties();
        }
    }
}
