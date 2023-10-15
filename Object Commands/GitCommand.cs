using Grooper;

#pragma warning disable CS1591
namespace GrooperGit
{
    /// <summary>Template for a custom for ObjectCommands under GitProject.</summary>
    /// <remarks>This abstract is here to minimize code rewrites for allowed exection.</remarks>
    public abstract class GitCommand : ObjectCommand<GrooperNode>
    {
        protected override bool CanExecute(GrooperNode Item)
        {
            return Item.GetType() == typeof(ProjectResourceAttribute) && Item.HasValue("Git");
        }
    }
}
