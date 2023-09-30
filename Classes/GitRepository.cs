using System;
using Grooper;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Security.Policy;

namespace GrooperGit
{
    /// <summary>
    /// Git Repository
    /// </summary>
    [DataContract, HelpBase]
    public class GitRepository : EmbeddedObject
    {
        private static GitRepository instance;
        private string _localPath = Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), "inetpub", "wwwroot", "Grooper"); 


        /// <inheritdoc/>
        public GitRepository(ConnectedObject Owner) : base(Owner)
        {

        }

        /// <summary>Represents the location of the local Git repository</summary>
        /// <remarks>The path is also where the required Git CLI runs</remarks>
        [DataMember, Viewable, DisplayName("Local Path")]
        public string localPath
        {
            get { return _localPath; }
            
            set 
            {   
                var directoryInfo = new DirectoryInfo(localPath);
                if (!directoryInfo.Exists) directoryInfo.Create();
                  
                _localPath = value; 
                //instance.OwnerNode.PropsDirty = true;   
            }
        }

        /// <summary>Represents the local path of the Git repository associated with the Grooper Project.</summary>
        /// <remarks>The repository URL points to the location where the Git project is hosted. Developers can use this URL to clone, fetch, or push changes to the repository. It's an essential link for collaboration and code management.</remarks>
        [DataMember, Viewable, DisplayName("Local Repository"), Required, Category("Local")]
        public string localRepository { get; set; }

        /// <summary>Manages Git branches within the specified GitProject.</summary>
        /// <remarks>Git branches allow developers to work on features or fixes in isolation, without affecting the main or other development lines. This ensures code stability and streamlined collaboration. More details can be found <a href="https://git-scm.com/docs/git-branch">here</a>.</remarks>
        public string localBranch { get; set; }

        /// <summary>Represents the remote path of the Git repository associated with the Grooper Project.</summary>
        /// <remarks>The repository URL points to the location where the Git project is hosted. Developers can use this URL to clone, fetch, or push changes to the repository. It's an essential link for collaboration and code management.</remarks>
        [DataMember, Viewable, DisplayName("Remo Repository"), Required, Category("Remote")]
        public string remoteRepository { get; set; }
        /// <inheritdoc/>
        public override ValidationErrorList ValidateProperties()
        {
            return base.ValidateProperties();
        }
    }
}
