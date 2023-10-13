using Grooper;
using System.Collections.Generic;

#pragma warning disable 1591

namespace GrooperGit
{
    public class Remove : ObjectCommand<GrooperNode>
    {
        protected override void Execute(GrooperNode Item)
        {
            List<string> Items = new List<string>();
            Items.Add(Item); // TODO figure out if Remove should use node, nodeasfile, or guid
            GitProject parentProject = (GitProject)Item.ParentProject;
            parentProject.Repository.Remove(Item);
        }

        protected override bool CanExecute(GrooperNode Item)
        {
            if(Item.GetValue("GitStatus") == "Staged") return true;
            return false;
        }
    }
}
