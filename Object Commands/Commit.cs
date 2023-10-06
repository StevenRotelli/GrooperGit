using Grooper;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace GrooperGit
{
    ///<summary>Executes a Git Commit operation on the specified GitProject [Object Command].</summary>
    ///<remarks>A Git Commit captures changes to files in the project, allowing for tracking
    ///of modifications and collaboration with other contributors. More details can be found <a href="https://git-scm.com/docs/git-commit">here</a>.</remarks>
    [DataContract, IconResource("Commit"), Category("Git")]
    public class Commit : ObjectCommand<GitProject>
    {
        public string _CommitMessage;

        [DisplayName("Commit Mesage"), Required, Viewable]
        public string CommitMessage
        {
            get { return _CommitMessage; }
            set { _CommitMessage = value; }
        }
        protected override void Execute(GitProject Item)
        {
            Item.Repository.Commit(_CommitMessage);
        }
        protected override bool CanExecute(GitProject Item)
        {
            if (Item.ChangedNodes.Count == 0) return true;
            return true; 
        }
    }
}
