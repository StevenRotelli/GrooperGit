using Grooper;
using System.Collections.Generic;
using System.ComponentModel;

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
            IEnumerable<string> branches = Instance.Repository.Branch("list").Split('\n');
            return Instance == null ? new List<string>() : branches;
        }
    }
}
