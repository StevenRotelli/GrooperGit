using Grooper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// A Git Project is a library of resources that can be version controlled and managed by other people.
/// It serves as the primary container in which design components are created and organized.
/// </summary>
/// <remarks>
/// Project 'resources' are configuration objects created by a designer, 
/// which define the connections, data structures, and logic used when processing documents through Grooper.
/// </remarks>
[Category("Miscellaneous"), IconResource("Git"), HomeFolder("Projects"), SettingsType(typeof(GitProject.GitProject_Properties))]
[ChildTypes(typeof(ProjectResourceAttribute), typeof(Folder))]
public class GitProject : Project {
    private GitProject_Properties Props => (GitProject_Properties)NodeProps;

    #region Props

    [DataContract]
    public class GitProject_Properties : Project_Properties {
        [DataMember] public string localBranch { get; set; }
        [DataMember] public string localRepository { get; set; }
        [DataMember] public string gitStatus { get; set; }
        [DataMember] public string gitChanges { get; set; }
        [DataMember] public string gitIgnore { get; set; }
        [DataMember] public string README { get; set; }
        [DataMember] public List<Guid> ChangedIds { get; set; }
        [DataMember] public string RemoteRepository { get; set; }

        public GitProject_Properties(GrooperNode owner) : base(owner) { }
    }

    public GitProject(GrooperDb gdb) : base(gdb) {
        Attributes = NodeAttributes.Sorted;
    }
    #endregion

    #region Constructers_and_Overrids
    public GitProject(GrooperDb gdb, NodeData data) : base(gdb, data) { }

    protected override bool? IsPropertyVisible(string propertyName) {

        return base.IsPropertyVisible(propertyName);
    }
    #endregion
    /// <summary>Represents the local path of the Git repository associated with the Grooper Project.</summary>
    /// <remarks>The repository URL points to the location where the Git project is hosted. Developers can use this URL to clone, fetch, or push changes to the repository. It's an essential link for collaboration and code management.</remarks>
    [DataMember, Viewable, DisplayName("Local Repository"), Required, Category("Git")]
    public string localRepository {
        get => Props.localRepository; 
        set { Props.localRepository = value; PropsDirty = true; }
    }

    [DataMember, Viewable, DisplayName("gitignore"), UI(typeof(PgTextEditor)), Category("Git")]
    public string gitIgnore {
        get => Props.gitIgnore;
        set { Props.gitIgnore = value; PropsDirty = true; }
    }

    [DataMember, Viewable, DisplayName("localBranch"), UI(typeof(PgTextEditor)), Category("Git")]
    public string localBranch {
        get => Props.localBranch;
        set { Props.localBranch = value; PropsDirty = true; }
    }
    [DataMember, Viewable, DisplayName("README"), UI(typeof(PgTextEditor)), Category("Git")]
    public string README {
        get => Props.README;
        set { Props.README = value; PropsDirty = true; }
    }
    public override ValidationErrorList ValidateProperties()
    {
        ValidationErrorList retVal = base.ValidateProperties();
        return retVal;
    }
}
