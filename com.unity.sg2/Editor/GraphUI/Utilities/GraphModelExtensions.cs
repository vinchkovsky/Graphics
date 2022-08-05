using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.ShaderGraph.GraphDelta;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace UnityEditor.ShaderGraph.GraphUI
{
    public static class GraphModelExtensions
    {
        public static GraphDataNodeModel CreateGraphDataNode(
            this IGraphModel graphModel,
            RegistryKey registryKey,
            string displayName = "",
            Vector2 position = default,
            SerializableGUID guid = default,
            SpawnFlags spawnFlags = SpawnFlags.Default
        )
        {
            return graphModel.CreateNode<GraphDataNodeModel>(
                displayName,
                position,
                guid,
                nodeModel =>
                {
                    if (spawnFlags.IsOrphan())
                    {
                        // Orphan nodes aren't a part of the graph, so we don't
                        // use an actual name in the graph data to represent them.
                        nodeModel.SetSearcherPreviewRegistryKey(registryKey);
                    }
                    else
                    {
                        var registry = ((ShaderGraphStencil)graphModel.Stencil).GetRegistry().Registry;
                        // Use this node's generated guid to bind it to an underlying element in the graph data.
                        var graphDataName = nodeModel.Guid.ToString();
                        ((ShaderGraphModel)graphModel).GraphHandler.AddNode(registryKey, graphDataName);
                        nodeModel.graphDataName = graphDataName;
                    }
                },
                spawnFlags
            );
        }

        public static GraphDataContextNodeModel CreateGraphDataContextNode(
            this ShaderGraphModel shaderGraphModel,
            string existingContextName,
            Vector2 position = default,
            SerializableGUID guid = default,
            SpawnFlags spawnFlags = SpawnFlags.Default
        )
        {
            return shaderGraphModel.CreateNode<GraphDataContextNodeModel>(
                existingContextName,
                position,
                guid,
                nodeModel =>
                {
                    nodeModel.graphDataName = existingContextName;
                },
                spawnFlags
            );
        }

        public static GraphDataNodeModel CreateGraphDataNode(
            this GraphNodeCreationData graphNodeCreationData,
            RegistryKey registryKey,
            string displayName
        )
        {
            return graphNodeCreationData.GraphModel.CreateGraphDataNode(
                registryKey,
                displayName,
                graphNodeCreationData.Position,
                graphNodeCreationData.Guid,
                graphNodeCreationData.SpawnFlags
            );
        }
    }
}
