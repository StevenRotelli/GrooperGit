using Grooper;
using System;
using System.IO;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization;

#pragma warning disable CS1591

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
        
        ///<summary>The local path of the Git repository</summary>
        [DisplayName("Local Path"), Required, Viewable]
        public string LocalPath { get; set;  }
        
        /// <summary>
        /// Code that will be executed when the [Object Command] is executed.
        /// </summary>
        /// <param name="item">The Grooper object that the command will execute against.</param>
        protected override void Execute(Project item)
        {
            string assemblyName = Assembly.GetExecutingAssembly().FullName;
            string typeName = assemblyName.Split(',')[0] + ".GitProject"; //derives node name from assembly incase Object Library is renamed

            string sqlCommand = $"UPDATE TreeNode SET TypeName=N'{typeName}' OUTPUT Inserted.RowVersion WHERE Id='{item.Id}'";
            Database.ExecuteScalar(sqlCommand);
            item.Database.PurgeCache();
            item.Database.ResetCache();
            
            GitProject projectNode = (GitProject)Database.GetNode(item.Id);
            //GitProject.GrooperNode_Properties projectNodeProperties = (GitProject.GrooperNode_Properties)projectNode.prop; 
 

            projectNode.LocalPath = LocalPath;
            projectNode.Repository.LocalBranch = "* master";
            projectNode.README = $"# {item.Name}";
            projectNode.GitIgnore = $"*.jpg{Environment.NewLine}";

            //string json = GlobalCode.CleanSql(ObjectSerializer.ToJson(projectNode.PropertiesJson));
            //Database.ExecuteScalar($"UPDATE TreeNode SET Properties=N'{json}' OUTPUT Inserted.RowVersion WHERE Id='{item.Id}'");
            projectNode.Repository.Init();

            foreach (GrooperNode childNode in projectNode.AllChildren)
            {
                NodeAsFile nodeAsFile = new NodeAsFile(childNode);
                nodeAsFile.WriteAll();
            }
        }
        public override ValidationErrorList ValidateProperties()
        {
            DirectoryInfo path = null;
            ValidationError pathErr = null;
            try
            {
                path = new DirectoryInfo(LocalPath);
                if (!path.Exists)
                {
                    pathErr = new ValidationError("Local Path", "Path not valid");
                    base.ValidateProperties().Add(pathErr);
                }
            }
            catch (Exception ex)
            {
                pathErr = new ValidationError("Local Path", $"Error: {ex.Message}");
                base.ValidateProperties().Add(pathErr);
            }
            
            return base.ValidateProperties();
        }

        protected override bool CanExecute(Project item)
        {
            return !item.TypeName.Contains("GitProject");
        }

    }
}
