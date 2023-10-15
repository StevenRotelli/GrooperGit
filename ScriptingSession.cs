using Grooper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GrooperGit
{
#pragma warning disable 1591
    public class ScriptingSession : ScriptObject
    {
        private ObjectLibrary ObjectLibrary;
        private GrooperRoot Root;
        public override bool Initialize(GrooperNode Item)
        {
            #region override allowed types
            ObjectLibrary = (ObjectLibrary)Item;
            Root = Item.Root;
            ProjectsFolder projectsFolder = Root.Projects;
            List<Type> allowedTypes = projectsFolder.AllowedChildTypes.ToList();

            if (allowedTypes.Any(t => t.FullName == typeof(GitProject).FullName)) { return true; }
            allowedTypes.Add(typeof(GitProject));
            projectsFolder.SetAllowedTypes(allowedTypes);
            #endregion
            return true;
        }

        public override bool Uninitialize()
        {
            return true;
        }
    }
}
