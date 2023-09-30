using Grooper;
using System;

//TODO:  See Custom Object Commands in Grooper x Change for more information:  https://xchange.grooper.com/discussion/777/custom-object-command#latest

namespace GrooperGit
{
    /// <summary>Template for a custom [Object Command].</summary>
    /// <remarks>"Type" should be replaced with the type of object the command will be executed against.</remarks>
    public abstract class GitCommand : ObjectCommand<Type> // TODO:  Replace Type with a Grooper Object
    {

        protected override bool CanExecute(Type Item)
        {
            return base.CanExecute(Item);
        }
        
    }
}
