using System;
using System.Collections.Generic;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.GraphToolsFoundation.Searcher;
using UnityEditor.ShaderGraph.GraphDelta;

namespace UnityEditor.ShaderGraph.GraphUI
{
    /// <summary>
    /// A RegistryNodeSearcherItem is a GraphNodeModelSearcherItem associated with a registry key. The key is exposed
    /// to make filtering easier.
    /// </summary>
    public class RegistryNodeSearcherItem : GraphNodeModelSearcherItem
    {
        readonly RegistryKey m_RegistryKey;

        public override string Name => m_RegistryKey.Name;

        public RegistryNodeSearcherItem(
            ShaderGraphModel graphModel,
            RegistryKey registryKey,
            string name,
            ISearcherItemData data = null,
            List<SearcherItem> children = null,
            Func<string> getName = null,
            string help = null
        ) : base(data,  creationData => graphModel.CreateGraphDataNode(registryKey, name, creationData.Position, creationData.Guid, creationData.SpawnFlags))
        {
            m_RegistryKey = registryKey;
        }
    }
}
