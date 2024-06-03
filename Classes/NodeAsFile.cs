using Grooper;
using System;
using System.IO;
using System.Collections.Generic;
#pragma warning disable CS1591
namespace GrooperGit
{
    public class ToGitRepo
    {
        public string LocalPath;
        public GrooperNode OwnerNode;
        public string PropertiesJson;
        public List<string> FileNames;
        public List<DirectoryInfo> Files;
        public GitProject ParentProject;

        public ToGitRepo(GrooperNode owner)
        {
            OwnerNode = owner;
            ParentProject = (GitProject)owner.ParentProject;
            // Path = ParentProject.Repository.LocalPath + BuildPathString(owner);
            LocalPath = Path.Combine(ParentProject.LocalPath, OwnerNode.Id.ToString());
            FileNames = new List<string>();
            FileNames.AddRange(owner.FileNames);
            PropertiesJson = owner.PropertiesJson;
        }

        // private string BuildPathString(GrooperNode node)
        // {
        //     GrooperNode curNode = node;
        //     string pathString = "";
        //     do
        //     {
        //         pathString = curNode.Name + "\\" + pathString;
        //         curNode = curNode.ParentNode;
        //     } while (curNode.GetType() != typeof(Project));
        //     return pathString;
        // }

        public void WriteAll()
        {
            DirectoryInfo nodePath = new DirectoryInfo(LocalPath);
            if (!nodePath.Exists)
            {
                try
                {
                    nodePath.Create();
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
                string path;

                foreach (FileStoreEntry file in OwnerNode.FileList)
                {
                    path = $"{LocalPath}\\{file.DisplayName}{file.FileExtension}";
                    using (FileStream fileStream = new FileStream(path, FileMode.Create))
                    {
                        file.OpenRead().CopyTo(fileStream);
                    }
                }
                string json = $"{{\n\"Name\" : \"{OwnerNode.DisplayName}\",\n";
                json += $"\"ParentId\" : \"{OwnerNode.ParentId}\",\n";
                json += OwnerNode.PropertiesJson.Substring(0, OwnerNode.PropertiesJson.Length);
                path = $"{LocalPath}\\{OwnerNode.Id}.json";
                File.WriteAllText(path, json);
            }
        }
    }
}
