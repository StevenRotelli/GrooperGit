using Grooper;

#pragma warning disable 1591

namespace GrooperGit
{
    public class Remove : ObjectCommand<GrooperNode>
    {
        protected override void Execute(GrooperNode Item)
        {
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
