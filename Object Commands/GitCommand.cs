using System;
using System.Collections.Generic;
using Grooper;
using Grooper.Core;



namespace GrooperGit
{
    /// <summary>Template for a custom for ObjectCommands under GitProject.</summary>
    /// <remarks>This abstract is here to minimize code rewrites for allowed exection.</remarks>
    public abstract class GitCommand : ObjectCommand<GrooperNode> 
    {
        
        protected override bool CanExecute(GrooperNode Item)
        {   
            if (Item.GetType() != typeof(ProjectResourceAttribute)) return false;
            if (!Item.HasValue("GitStatus")) return false;          
            return true;
        }

    }
}
