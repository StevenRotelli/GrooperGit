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
        private string _CommitMessage;

        ///<inheritdoc/>
        [DisplayName("Commit Mesage"), Required, Viewable]
        public string CommitMessage
        {
            get => _CommitMessage;
            set => _CommitMessage = value;
        }

        ///<summary></summary>
        ///<remarks></remarks>
        protected override void Execute(GitProject Item)
        {
            Item.Repository.Commit(_CommitMessage);
        }

        ///<summary>Will not execute if there are no staged changes.</summary>
        protected override bool CanExecute(GitProject Item)
        {
            return Item.ChangedNodes.Count != 0;
        }
    }
}
