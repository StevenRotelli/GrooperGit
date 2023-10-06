using Grooper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrooperGit 
{
    /// <summary>
    /// Creates a dropdown list of branches from the connected Git Project.
    /// </summary>
    public class BranchListConverter : PgListConverter<GitProject, string>
    {
        /// <inheritdoc/>
        protected override IEnumerable<string> GetListItems(GitProject Instance, PropertyDescriptor pd)
        {
            if (Instance == null)  return new List<string>(); 

            GitShell gitShell = new GitShell(Instance.LocalRepository);
            return gitShell.ListBranches();
        }
    }
}
