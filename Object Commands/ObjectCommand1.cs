using Grooper;
using System;

//TODO:  See Custom Object Commands in Grooper x Change for more information:  https://xchange.grooper.com/discussion/777/custom-object-command#latest

namespace GrooperGit
{
    /// <summary>Template for a custom [Object Command].</summary>
    /// <remarks>"Type" should be replaced with the type of object the command will be executed against.</remarks>
    public class ObjectCommand1 : ObjectCommand<Type> // TODO:  Replace Type with a Grooper Object
    {

        /// <summary>Code that will be executed when the [Object Command] is executed.</summary>
        /// <param name="Item">The Grooper object that the command will execute against.</param>
        protected override void Execute(Type Item) // TODO:  Replace Type with a Grooper Object
        {


        }


    }
}
