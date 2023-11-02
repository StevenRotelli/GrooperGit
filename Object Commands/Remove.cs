using Grooper;
using System;
using System.Collections.Generic;

using static GrooperGit.Utils;

#pragma warning disable 1591

namespace GrooperGit
{
    public class Remove : ObjectCommand<GrooperNode>
    {
        protected override void Execute(GrooperNode Item)
        {
            string name = "sean";
            name = MonoSpace(name);
            GitProject parentProject = (GitProject)Item.ParentProject;
            parentProject.Repository.Remove(Item);
        }

        protected override bool CanExecute(GrooperNode Item)
        {
            if(Item.GetValue("Git") == "Staged") return true;
            return false;
        }
    }
}
