using Grooper.Core;
using Grooper;  
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Security.AccessControl;

namespace GrooperGit
{
    internal class ChangedNodeListEditor : NodeListEditor<GrooperNode>
    {
        public override IEnumerable<GrooperNode> GetBaseNodes(Type PropertyType, ConnectedObject ConnectedItem)
        {
            return GetModified(ConnectedItem);
        }
        protected static IEnumerable<GrooperNode> GetModified(ConnectedObject ConnectedItem)
        {
            List<GrooperNode> RetVal = new List<GrooperNode>();
            foreach (GrooperNode node in (ConnectedItem as GrooperNode))
            {
                if (node.HasValue("GitStatus"))
                {
                    RetVal.Add(node);
                }
            }
            return RetVal;
        }

    }

}
