using System;
using System.Collections.Generic;
using Grooper.Core;
using Grooper;

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
            foreach (GrooperNode node in ConnectedItem as GrooperNode)
            {
                if (node.HasValue("Git"))
                {
                    RetVal.Add(node);
                }
            }
            return RetVal;
        }

    }

}
