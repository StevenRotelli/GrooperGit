using Grooper;
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;

#pragma warning disable CS1591
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
    [Category("Miscellaneous"), IconResource("Git"), HomeFolder("Projects"), SettingsType(typeof(GitProject_Properties))]
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
            [DataMember] public string LocalPath { get; set; }
            [DataMember] public string GitChanges { get; set; }
            [DataMember] public string GitIgnore { get; set; }
            [DataMember] public string README { get; set; }
            [DataMember] public List<Guid> ChangedIds { get; set; }
            [DataMember] public string RemoteURL { get; set; }

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

        /// <summary>Represents the local path of the Git repository associated with the Grooper Project.</summary>
        /// <remarks>The repository URL points to the location where the Git project is hosted. Developers can use this URL to clone, fetch, or push changes to the repository. It's an essential link for collaboration and code management.</remarks>
        [DataMember, Viewable, Category("Git")]
        public string LocalPath { get => Props.LocalPath; set { Props.LocalPath = value; PropsDirty = true; } }

        /// <summary>Implements <a target="_blank" href="https://en.wikipedia.org/wiki/OAuth">OAuth Authentication</a> as defined in 
        /// <a target="_blank" href="https://oauth.net/2/">OAuth 2.0</a>.</summary>
        [DataMember, Viewable, DisplayName("Remote URL"), DV("https://github.com/")]
        public string RemoteURL
        {
            get => Props.RemoteURL; set { Props.RemoteURL = value; PropsDirty = true; }
        }

        [DataMember, Viewable, RequiresUI, UI(typeof(ChangedNodeListEditor)), TypeConverter(typeof(PgCollectionConverter)), Category("Git")]
        public List<GrooperNode> ChangedNodes
        {
            get => Database.GetNodes<GrooperNode>(Props.ChangedIds);
            set { Props.ChangedIds = new List<Guid>(value.Where(Item => Item.Id != Id).Select(Item => Item.Id)); PropsDirty = true; }
        }

        [DataMember, Viewable, DisplayName("Repo"), TypeConverter(typeof(ExpandableConverter)), Category("Git")]
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
            get => File.Exists($"{LocalPath}\\README.MD") ? File.ReadAllText($"{LocalPath}\\README.MD") : "";
            //IEnumerable<GrooperNode> allResourceFiles = get_AllChildrenOfType(typeof(ResourceFile));
            //ResourceFile readmeFile = (ResourceFile)allResourceFiles.FirstOrDefault(rf => rf.Name.Equals("README.MD", StringComparison.OrdinalIgnoreCase));
            //return readmeFile != null ? readmeFile.ReadAsText() : "";
            set
            {
                File.WriteAllText($"{LocalPath}\\README.MD", value);
                Props.GitIgnore = value;
                PropsDirty = true;

                //IEnumerable<GrooperNode> allResourceFiles = get_AllChildrenOfType(typeof(ResourceFile));
                //ResourceFile readmeFile = (ResourceFile)allResourceFiles.FirstOrDefault(rf => rf.Name.Equals("README.MD", StringComparison.OrdinalIgnoreCase));
                //if (readmeFile == null)
                //{
                //    readmeFile = new ResourceFile(Database);
                //    if (AppendNode(readmeFile) == false)
                //    {
                //        throw new Exception("Unable to append ResourceFile node") { };
                //    }

                //    readmeFile.SaveTextFile("README.MD", value, true);
                //}
                //readmeFile.SaveTextFile("README.MD", value, true); PropsDirty = true;
            }
        }

        [DataMember, Viewable, DisplayName("gitignore"), UI(typeof(PgTextEditor)), Category("Git")]
        public string GitIgnore
        {
            get => File.Exists($"{LocalPath}\\.gitignore") ? File.ReadAllText($"{LocalPath}\\.gitignore") : "";
            set
            {
                File.WriteAllText($"{LocalPath}\\.gitignore", value);
                Props.GitIgnore = value;
                PropsDirty = true;
            }
        }

        public override ValidationErrorList ValidateProperties()
        {
            ValidationErrorList retVal = base.ValidateProperties();
            return retVal;
        }
    }
}
