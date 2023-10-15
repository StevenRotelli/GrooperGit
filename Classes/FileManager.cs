using System;
using System.IO;
using System.Collections.Generic;
using Grooper;
namespace GrooperGit
{
    /// <summary>
    /// A Class to maintain a file abstract of the Grooper Tree Under the git project in order to leverage GIT commands against the Grooper Nodes.
    /// </summary>
    /// <remarks>
    /// This is because Grooper Nodes aren't files and need to be able to detect differences as files and compare differences as files.
    /// </remarks>
    [Obsolete("FileManager is depricated, factory replaced for mutator NodeAsfile")]
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
                GitProject gitProject = (GitProject)grooperNode;

                // if (string.IsNullOrEmpty(gitProject?.LocalRepository)) return;

                foreach (GrooperNode node in gitProject.AllChildren)
                {
                    if (!File.Exists(""))
                    {
                        node.SetValue("Git", "New");
                        continue;
                    }
                    NodeToFile(node);
                }
            }
        }
        ///<summary>Checks the state of the node </summary>
        ///<remarks>will create the necessary files in the git repository if needed, and 
        ///also set the node value to the appropriate git status</remarks>
        ///<param name="SyncChildrenToo">optionally sync all children of node</param>
        ///<param name="node">The node to be synced with the filesystem></param>
        public static void Sync(GrooperNode node, bool SyncChildrenToo = false)
        {
            List<GrooperNode> nodes = new List<GrooperNode>();
            nodes.Add(node);
            if (SyncChildrenToo == true)
            {
                nodes.AddRange(node.AllChildren);
            }
            foreach(GrooperNode n in nodes)
            {
                NodeToFile(n);
            }
        }
        
        public static void NodeToFile(object obj)
        {
            if (obj is GitProject)
            {
                var node = obj as GitProject;
                // File.WriteAllLines(Path.Combine(node.LocalRepository, node.Id + ".json"), node.PropertiesJson.Split('\n'));
            }
            else
            {
                var node = obj as GrooperNode;
                var parentNode = node.ParentProject as GitProject;
                // File.WriteAllLines(Path.Combine(parentNode.LocalRepository, node.Id + ".json"), node.PropertiesJson.Split('\n'));
            }
        }
    }
}
