using Grooper;
using System.Runtime.Serialization;

#pragma warning disable CS1591
namespace GrooperGit
{
    /// <summary>Executes a Git Stage operation, preparing changes for commit in the specified GitProject [Object Command].</summary>
    /// <remarks>Git Staging allows you to prepare changes for a commit, helping you organize modifications and facilitating code review. Learn more about Git Staging <a href="https://git-scm.com/docs/git-stage">here</a>.</remarks>
    [DataContract, IconResource("Stage")]
    public class Stage : GitCommand
    {
        /// <inheritdoc/>
        protected override void Execute(GrooperNode Item)
        {
            GitProject parentProject = (GitProject)Item.ParentProject;
            if (!parentProject.ChangedNodes.Contains(Item))
            {
                parentProject.ChangedNodes.Add(Item);
            }
        }

        protected override bool CanExecute(GrooperNode Item)
        {
            return Item.GetValue("Git") == "Modified" && base.CanExecute(Item);
        }
    }
}

