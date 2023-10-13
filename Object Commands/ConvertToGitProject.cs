using Grooper;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization;
using static Grooper.GlobalCode;
#pragma warning disable 1591

namespace GrooperGit
{
    /// <summary>
    /// Executes a publish to git, and creates a git repository on a Project [Object Command].
    /// </summary>
    /// <remarks>
    /// Transforms the node type from Project to GitProject, and begins a repository connection.
    /// <a href="https://git-scm.com/book/en/v2/Git-Basics-Getting-a-Git-Repository">More info</a>.
    /// </remarks>
    [DataContract, IconResource("Git"), DisplayName("Publish to Git Repository"), Category("Share")]
    public class ConverToGitProject : ObjectCommand<Project>
    {
        [DataMember, DefaultValue("C:\\GitRepositories\\")]
        public string LocalRepository { get; set; }

        [DisplayName("Local Repository"), Required, Viewable]
        public string _LocalRepository
        {
            get { return LocalRepository; }
            set { LocalRepository = value; }
        }

        /// <summary>
        /// Code that will be executed when the [Object Command] is executed.
        /// </summary>
        /// <param name="item">The Grooper object that the command will execute against.</param>
        protected override void Execute(Project item)
        {
            string assemblyName = Assembly.GetExecutingAssembly().FullName;
            string typeName = assemblyName.Split(',')[0] + ".GitProject";

            Database.ExecuteScalar($"UPDATE TreeNode SET TypeName=N'{typeName}' OUTPUT Inserted.RowVersion WHERE Id='{item.Id}'");
            item.Database.PurgeCache();
            item.Database.ResetCache();

            GitProject projectNode = (GitProject)Database.GetNode(item.Id);
            //GitProject.GrooperNode_Properties projectNodeProperties = (GitProject.GrooperNode_Properties)projectNode.prop; 

            projectNode.LocalRepository = LocalRepository;
            //projectNode.localBranch = "* master";
            projectNode.README = $"# {item.Name}";
            projectNode.GitIgnore = $"*.jpg{Environment.NewLine}";

            //string json = GlobalCode.CleanSql(ObjectSerializer.ToJson(projectNode.PropertiesJson));
            //Database.ExecuteScalar($"UPDATE TreeNode SET Properties=N'{json}' OUTPUT Inserted.RowVersion WHERE Id='{item.Id}'");

            var gitConsole = new GitShell(LocalRepository);
            gitConsole.Init();

            FileManager.NodeToFile(projectNode);
            foreach (var childNode in projectNode.AllChildren)
            {
                FileManager.NodeToFile(childNode);
            }

            gitConsole.Add(".");
            gitConsole.Commit("Repository Initialized");
        }

        protected override bool CanExecute(Project item)
        {
            if (item.TypeName.Contains("GitProject")) return false;
            return true;
        }

    }
}
