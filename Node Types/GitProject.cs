using Grooper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace GrooperGit
{
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
    public class GitProject : Project
    {
        private GitProject_Properties Props => (GitProject_Properties)NodeProps;

        #region Props

        [DataContract]
        public class GitProject_Properties : Project_Properties
        {
            [DataMember] public GitRepository Repository { get; set; }
            [DataMember] public string GitStatus { get; set; }
            [DataMember] public string LocalRepository { get; set; }
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
        #endregion

        #region Constructers_and_Overrids
        public GitProject(GrooperDb gdb, NodeData data) : base(gdb, data) { }

        protected override bool? IsPropertyVisible(string propertyName)
        {

            return base.IsPropertyVisible(propertyName);
        }
        #endregion

        [DataMember, Viewable, DisplayName("gitignore"), UI(typeof(PgTextEditor)), Category("Git")]
        public string gitIgnore
        {
            get => Props.gitIgnore; set { Props.gitIgnore = value; PropsDirty = true; }
        }
        
        [DataMember, Viewable,DisplayName("Repo"),TypeConverter(typeof(ExpandableConverter)) ,Category("Git")]
        public GitRepository Repository
        {
            get 
            { 
                if (Props.Repository == null)
                {
                    Props.Repository = new GitRepository(this);
                }
                return Props.Repository; 
            } 
            set { Props.Repository = value; PropsDirty = true; }

        }
        [DataMember, Viewable, DisplayName("README"), UI(typeof(MarkDownEditor)), Category("Git")]
        public string README
        {
            get 
            {
                var allResourceFiles = this.get_AllChildrenOfType(typeof(ResourceFile));
                ResourceFile readmeFile = (ResourceFile)allResourceFiles.FirstOrDefault(rf => rf.Name.Equals("README.MD", StringComparison.OrdinalIgnoreCase));
                if (readmeFile != null) return readmeFile.ReadAsText(); 
                return "";
            } 
            set 
            { 
                var allResourceFiles = this.get_AllChildrenOfType(typeof(ResourceFile));
                ResourceFile readmeFile = (ResourceFile)allResourceFiles.FirstOrDefault(rf => rf.Name.Equals("README.MD", StringComparison.OrdinalIgnoreCase));
                if (readmeFile == null)
                {
                    readmeFile = new ResourceFile(this.Database);
                    this.AppendNode(readmeFile);
                    readmeFile.SaveTextFile("README.MD", value, true);
                }
                readmeFile.SaveTextFile("README.MD", value, true); PropsDirty = true; 
            }
        }

        [DataMember, Viewable, RequiresUI, UI(typeof(ChangedNodeListEditor)), TypeConverter(typeof(PgCollectionConverter))]
        public List<GrooperNode> ChangedNodes
        {
            get => Database.GetNodes<GrooperNode>(Props.ChangedIds);
            set { Props.ChangedIds = new List<Guid>(value.Where(Item => Item.Id != Id).Select(Item => Item.Id)); PropsDirty = true; }
        }
        public override ValidationErrorList ValidateProperties()
        {
            ValidationErrorList retVal = base.ValidateProperties();
            return retVal;
        }
    }
}
