using Grooper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;


// Assuming necessary attributes and classes are available in imported namespaces

[Category("Miscellaneous")]
[IconResource("Git")]
[HomeFolder("Projects")]
[SettingsType(typeof(GitProject.GitProject_Properties))]
[ChildTypes(typeof(ProjectResourceAttribute), typeof(Folder))]

/// <summary>
/// A Git Project is a library of resources that can be version controlled and managed by other people.
/// It serves as the primary container in which design components are created and organized.
/// </summary>
/// <remarks>
/// Project 'resources' are configuration objects created by a designer, 
/// which define the connections, data structures, and logic used when processing documents through Grooper.
/// </remarks>
public class GitProject : Project
{
    private GitProject_Properties Props => (GitProject_Properties)NodeProps;

    [DataContract]
    public class GitProject_Properties : Project_Properties
    {
        /// <summary>
        /// The branch of the local repository
        /// </summary>
        [DataMember] public string gitBranch { get; set; }
        /// <summary>
        /// The path to the local repository
        /// </summary>
        [DataMember] public string localRepository { get; set; }
        [DataMember] public string gitStatus { get; set; }
        [DataMember] public string gitChanges { get; set; }
        [DataMember] public string gitIgnore { get; set; }
        [DataMember] public string README { get; set; }
        [DataMember] public List<Guid> ChangedIds { get; set; }
        [DataMember] public string RemoteRepository { get; set; }

        public GitProject_Properties(GrooperNode owner) : base(owner) { }
    }

    public GitProject(GrooperDb gdb) : base(gdb)
    {
        Attributes = NodeAttributes.Sorted;
    }

    public GitProject(GrooperDb gdb, NodeData data) : base(gdb, data) { }

    protected override bool? IsPropertyVisible(string propertyName)
    {
        // Uncomment and adjust as necessary
        //switch (propertyName)
        //{
        //    case nameof(ContextElement): return !string.IsNullOrEmpty(OutputElement);
        //    case nameof(ValuePattern): return !IsWinForms;
        //    case nameof(Pattern): return IsWinForms;
        //    default: return base.IsPropertyVisible(propertyName);
        //}
        return base.IsPropertyVisible(propertyName);
    }

    // The rest of your properties, converted to C#...

    // Example:
    /// <summary>Represents the local path of the Git repository associated with the Grooper Project.</summary>
    /// <remarks>The repository URL points to the location where the Git project is hosted. Developers can use this URL to clone, fetch, or push changes to the repository. It's an essential link for collaboration and code management.</remarks>
    [DataMember]
    [Viewable]
    [DisplayName("LocalRepositoryRepositiory")]
    [Required]
    [Category("Git")]
    public string LocalRepository
    {
        get
        {
            return Props.localRepository;
        }
    }

    // ... etc ...

    private bool IsValidGitURL(string url)
    {
        Uri result;
        if (Uri.TryCreate(url, UriKind.Absolute, out result))
        {
            return url.EndsWith(".git") || url.Contains("github.com/");
        }
        return false;
    }

    private string SanitizePath(string path)
    {
        return path.EndsWith("/") ? path : path + "/";
    }

    public override ValidationErrorList ValidateProperties()
    {
        ValidationErrorList retVal = base.ValidateProperties();
        // Uncomment and adjust as necessary
        //if (Props.gitUrl != null)
        //{
        //    if (!IsValidGitURL(GitRepository))
        //    {
        //        retVal.Add("GitRepository", "Invalid git URL");
        //    }
        //}
        return retVal;
    }
}
