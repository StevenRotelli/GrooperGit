using Grooper;
using System.IO;
using System.Collections.Generic;

namespace GrooperGit
{
    class NodeAsFile //TODO I have no idea what is going on here
    {
        public string Path;
        public string PropertiesJson;
        public List<string> FileNames;
        public List<DirectoryInfo> Files;
        public GitProject ParentProject;
        public NodeAsFile(GrooperNode owner)
        {
            ParentProject = (GitProject)owner.ParentProject;
            Path = ParentProject.Repository.LocalPath + BuildPathString(owner);
            FileNames = (List<string>)owner.FileNames;
            PropertiesJson = owner.PropertiesJson;
        }

        private string BuildPathString(GrooperNode node)
        {
            GrooperNode curNode = node;
            string pathString = "";
            do
            {
                pathString = curNode.Name + "\\" + pathString;
                curNode = curNode.ParentNode;
            } while (curNode.GetType() != typeof(Project));
            return pathString;
        }
    }
}
