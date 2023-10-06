using System.IO;
using Grooper;
namespace GrooperGit
{
    /// <summary>
    /// A Class to maintain a file abstract of the Grooper Tree Under the git project in order to leverage GIT commands against the Grooper Nodes.
    /// </summary>
    /// <remarks>
    /// This is because Grooper Nodes aren't files and need to be able to detect differences as files and compare differences as files.
    /// </remarks>
    public class FileManager
    {
        /// <summary>
        /// Syncs the active node settings of all children in the project and marks their appropriate GitStatus.
        /// </summary>
        /// <param name="grooperNode">The GrooperNode to be synced.</param>
        public static void SyncProject(GrooperNode grooperNode)
        {
            if (grooperNode.TypeDisplayName == "GitProject")
            {
                var gitProject = (GitProject)grooperNode;

                if (string.IsNullOrEmpty(gitProject?.LocalRepository)) return;

                var gitConsole = new GitShell(gitProject.LocalRepository);
                foreach (var node in gitProject.AllChildren)
                {
                    if (!File.Exists(""))
                    {
                        node.SetValue("GitStatus", "New");
                        continue;
                    }
                    FileManager.NodeToFile(node);
                }
            }
        }
        public static void NodeToFile(object obj)
        {
            if (obj is GitProject)
            {
                var node = obj as GitProject;
                File.WriteAllLines(Path.Combine(node.LocalRepository, node.Id + ".json"), node.PropertiesJson.Split('\n'));
            }
            else
            {
                var node = obj as GrooperNode;
                var parentNode = node.ParentProject as GitProject;
                File.WriteAllLines(Path.Combine(parentNode.LocalRepository, node.Id + ".json"), node.PropertiesJson.Split('\n'));
            }
        }
    }
}