using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Profiling;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;
using UnityEditor.ShaderGraph.GraphDelta;
using UnityEditor.ShaderGraph.Serialization;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace UnityEditor.ShaderGraph.GraphUI
{
    public enum PropagationDirection
    {
        Upstream,
        Downstream
    }

    [Serializable]
    class MainPreviewData
    {
        public SerializableMesh serializedMesh = new ();
        public bool preventRotation;

        public int width = 125;
        public int height = 125;

        [NonSerialized]
        public Quaternion rotation = Quaternion.identity;

        [NonSerialized]
        public float scale = 1f;

        public void Initialize()
        {
            if (serializedMesh.IsNotInitialized)
            {
                // Initialize the sphere mesh as the default
                Mesh sphereMesh = Resources.GetBuiltinResource(typeof(Mesh), $"Sphere.fbx") as Mesh;
                serializedMesh.mesh = sphereMesh;
            }
        }
    }

    public class ShaderGraphModel : GraphModel
    {
        [NonSerialized]
        public GraphModelStateComponent graphModelStateComponent;
        [NonSerialized]
        public SelectionStateComponent selectionStateComponent;

        public GraphHandler GraphHandler => ShaderGraphAssetModel.GraphHandler;

        public ShaderGraphAssetModel ShaderGraphAssetModel => Asset as ShaderGraphAssetModel;

        public Registry RegistryInstance => ((ShaderGraphStencil)Stencil).GetRegistry();

        #region MainPreviewData
        MainPreviewData m_MainPreviewData = new ();

        internal MainPreviewData mainPreviewData => m_MainPreviewData;

        public void SetPreviewMesh(Mesh newPreviewMesh)
        {
            m_MainPreviewData.serializedMesh.mesh = newPreviewMesh;
        }

        public void SetPreviewScale(float newPreviewScale)
        {
            m_MainPreviewData.scale = newPreviewScale;
        }

        public void SetPreviewRotation(Quaternion newRotation)
        {
            m_MainPreviewData.rotation = newRotation;
        }

        public void SetPreviewSize(Vector2 newPreviewSize)
        {
            m_MainPreviewData.width = Mathf.FloorToInt(newPreviewSize.x);
            m_MainPreviewData.height = Mathf.FloorToInt(newPreviewSize.y);
        }

        public void SetPreviewRotationLocked(bool preventRotation)
        {
            m_MainPreviewData.preventRotation = preventRotation;
        }

        #endregion
        [SerializeField]
        string m_GraphJSON = new (String.Empty);

        [SerializeField]
        string m_TargetSettingsJSON = new (String.Empty);

        public string graphModelJson => m_GraphJSON;

        public string targetSettingsJson => m_TargetSettingsJSON;

        public void SaveShaderGraphStateToJson()
        {
            m_GraphJSON = GraphHandler.ToSerializedFormat();

            m_TargetSettingsJSON = MultiJson.Serialize(ShaderGraphAssetModel.targetSettingsObject);
        }

        public void RestoreShaderGraphStateFromJson()
        {
            ShaderGraphAssetModel.GraphHandler = GraphHandler.FromSerializedFormat(graphModelJson, RegistryInstance);
            ShaderGraphAssetModel.GraphHandler.ReconcretizeAll();
            MultiJson.Deserialize(ShaderGraphAssetModel.targetSettingsObject, targetSettingsJson);
        }

        public override IEdgeModel CreateEdge(IPortModel toPort, IPortModel fromPort, SerializableGUID guid = default)
        {
            IPortModel resolvedEdgeSource = null;
            List<IPortModel> resolvedEdgeDestinations = null;
            using var graphModelStateUpdater = graphModelStateComponent.UpdateScope;
            {
                resolvedEdgeSource = HandleRedirectNodesCreation(graphModelStateUpdater, toPort, fromPort, out resolvedEdgeDestinations);
            }

            var edgeModel = base.CreateEdge(toPort, fromPort, guid);

            if (resolvedEdgeSource is GraphDataPortModel fromDataPort)
            {
                // Make the corresponding connections in CLDS data model
                foreach (var toDataPort in resolvedEdgeDestinations.OfType<GraphDataPortModel>())
                {
                    // Validation should have already happened in GraphModel.IsCompatiblePort.
                    Assert.IsTrue(TryConnect(fromDataPort, toDataPort));
                }
            }

            return edgeModel;

        }

        static IPortModel HandleRedirectNodesCreation(GraphModelStateComponent.StateUpdater graphUpdater, IPortModel toPort, IPortModel fromPort, out List<IPortModel> resolvedDestinations)
        {
            var resolvedSource = fromPort;
            resolvedDestinations = new List<IPortModel>();

            if (toPort.NodeModel is RedirectNodeModel toRedir)
            {
                resolvedDestinations = toRedir.ResolveDestinations().ToList();

                // Update types of descendant redirect nodes.
                foreach (var child in toRedir.GetRedirectTree(true))
                {
                    child.UpdateTypeFrom(fromPort);
                    graphUpdater.MarkChanged(child);
                }
            }
            else
            {
                resolvedDestinations.Add(toPort);
            }

            if (fromPort.NodeModel is RedirectNodeModel fromRedir)
            {
                resolvedSource = fromRedir.ResolveSource();
            }

            return resolvedSource;
        }

        public override IEdgeModel DuplicateEdge(IEdgeModel sourceEdge, INodeModel targetInputNode, INodeModel targetOutputNode)
        {
            return base.DuplicateEdge(sourceEdge, targetInputNode, targetOutputNode);
        }

        public override IReadOnlyCollection<IGraphElementModel> DeleteEdges(IReadOnlyCollection<IEdgeModel> edgeModels)
        {
            using var graphModelStateUpdater = graphModelStateComponent.UpdateScope;
            {
                foreach (var model in edgeModels)
                {
                    if (!model.IsDeletable())
                        continue;

                    // Reset types on disconnected redirect nodes.
                    if (model.ToPort.NodeModel is RedirectNodeModel redirect)
                    {
                        graphModelStateUpdater.MarkChanged(redirect);
                        redirect.ClearType();
                    }

                    graphModelStateUpdater.MarkDeleted(model);

                    // Remove the edge from CLDS
                    GraphHandler.RemoveEdge(model.FromPortId, model.ToPortId);
                }
            }
            return edgeModels;
        }

        public override IReadOnlyCollection<IGraphElementModel> DeleteNodes(IReadOnlyCollection<INodeModel> nodeModels, bool deleteConnections)
        {
            var redirects = new List<RedirectNodeModel>();
            var nonRedirects = new List<INodeModel>();

            foreach (var model in nodeModels)
            {
                switch (model)
                {
                    case RedirectNodeModel redirectModel:
                        redirects.Add(redirectModel);
                        break;
                    default:
                        nonRedirects.Add(model);
                        break;
                }
            }

            List<IGraphElementModel> deletedModels;
            using var graphModelStateUpdater = graphModelStateComponent.UpdateScope;
            {
                // Partition out redirect nodes because they get special delete behavior.
                deletedModels = HandleRedirectNodesDeletion(graphModelStateUpdater, redirects);
            }

            // Delete everything else as usual.
            deletedModels.AddRange(base.DeleteNodes(nonRedirects, false));

            using var selectionStateUpdater = selectionStateComponent.UpdateScope;
            {
                var selectedModels = deletedModels.Where(m => selectionStateComponent.IsSelected(m)).ToList();
                if (selectedModels.Any())
                {
                    // Deselect any now-deleted models that were selected
                    selectionStateUpdater.SelectElements(selectedModels, false);
                }
            }

            // After all redirect nodes handling and deletion has been handled above
            // Mark the nodes for deletion (actual CLDS node removal happens in GraphModelStateObserver)
            foreach (var model in deletedModels)
            {
                graphModelStateUpdater.MarkDeleted(model);
            }

            return deletedModels;
        }

        List<IGraphElementModel> HandleRedirectNodesDeletion(
            GraphModelStateComponent.StateUpdater graphModelStateUpdater,
            List<RedirectNodeModel> redirects)
        {
            // If redirect node is deleted, connect the incoming edge to all outgoing edges of that redirect node
            foreach (var redirect in redirects)
            {
                var inputEdgeModel = redirect.GetIncomingEdges().FirstOrDefault();
                var outputEdgeModels = redirect.GetOutgoingEdges().ToList();

                // Delete the redirect node edges first
                base.DeleteEdges(outputEdgeModels);
                this.DeleteEdge(inputEdgeModel);

                graphModelStateUpdater.MarkDeleted(inputEdgeModel);
                graphModelStateUpdater.MarkDeleted(outputEdgeModels);

                if (inputEdgeModel == null || !outputEdgeModels.Any()) continue;

                foreach (var outputEdgeModel in outputEdgeModels)
                {
                    var edge = CreateEdge(outputEdgeModel.ToPort, inputEdgeModel.FromPort);
                    graphModelStateUpdater.MarkNew(edge);
                }
            }

            // Don't delete connections for redirects, because we may have edges we want to preserve
            // Edges we don't need were already deleted in the above loop.
            var deletedModels = base.DeleteNodes(redirects, false).ToList();
            return deletedModels;
        }

        public GraphDataNodeModel CreateGraphDataNode(RegistryKey registryKey, string nodeName, Vector2 position = default, SerializableGUID guid = default, SpawnFlags spawnFlags = SpawnFlags.None)
        {
            var nodeModel = this.CreateNode<GraphDataNodeModel>(
                nodeName,
                position,
                guid,
                nodeModel =>
                {
                    if (spawnFlags.IsOrphan())
                    {
                        // Orphan nodes aren't a part of the graph, so we don't use an actual name in the graph data
                        // to represent them.
                        nodeModel.SetSearcherPreviewRegistryKey(registryKey);
                    }
                    else
                    {
                        // Use this node's generated guid to bind it to an underlying element in the graph data.
                        var graphDataName = guid.ToString();
                        // TODO: Replace with boxing solution
                        GraphHandler.AddNode(registryKey, graphDataName);
                        nodeModel.graphDataName = graphDataName;
                    }
                },
                spawnFlags
            );

            return nodeModel;
        }

        public GraphDataContextNodeModel CreateGraphDataContextNode(
            string existingContextName,
            Vector2 position = default,
            SerializableGUID guid = default,
            SpawnFlags spawnFlags = SpawnFlags.Default
        )
        {
            return this.CreateNode<GraphDataContextNodeModel>(
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

        public override INodeModel CreateNode(Type nodeTypeToCreate, string nodeName, Vector2 position, SerializableGUID guid = default, Action<INodeModel> initializationCallback = null, SpawnFlags spawnFlags = SpawnFlags.None)
        {
            return base.CreateNode(nodeTypeToCreate, nodeName, position, guid, initializationCallback, spawnFlags);
        }

        public override IConstantNodeModel CreateConstantNode(TypeHandle constantTypeHandle, string constantName, Vector2 position, SerializableGUID guid = default, Action<IConstantNodeModel> initializationCallback = null, SpawnFlags spawnFlags = SpawnFlags.None)
        {
            return base.CreateConstantNode(constantTypeHandle, constantName, position, guid, initializationCallback, spawnFlags);
        }

        public override INodeModel DuplicateNode(INodeModel sourceNode, Vector2 delta)
        {
            return base.DuplicateNode(sourceNode, delta);
        }

        protected override Type GetEdgeType(IPortModel toPort, IPortModel fromPort)
        {
            return typeof(GraphDataEdgeModel);
        }

        public override Type GetSectionModelType()
        {
            return typeof(SectionModel);
        }

        public override void OnEnable()
        {
            base.OnEnable();

            // Assigning this value manually as section models are setup by default to have the asset model reference serialized in, but we modified GTF to prevent that
            foreach (var sectionModel in SectionModels)
            {
                // sectionModel.AssetModel = AssetModel;
            }

            foreach (var variableDeclarationModel in VariableDeclarations)
            {
                // Variable declarations need to be given a valid GraphHandler now so the blackboard can build the
                // correct fields.
                if (variableDeclarationModel.InitializationModel is BaseShaderGraphConstant cldsConstant)
                {
                    cldsConstant.Initialize(GraphHandler, cldsConstant.NodeName, cldsConstant.PortName);
                }
            }

            var contextNames = GraphHandler
                .GetNodes()
                .Where(nodeHandler => nodeHandler.GetRegistryKey().Name == Registry.ResolveKey<ContextBuilder>().Name)
                .Select(nodeHandler => nodeHandler.ID.LocalPath)
                .ToList();

            foreach (var localPath in contextNames)
            {
                GraphHandler.ReconcretizeNode(localPath);

                if (!NodeModels.Any(nodeModel =>
                        nodeModel is GraphDataContextNodeModel contextNodeModel &&
                        contextNodeModel.graphDataName == localPath))
                {
                    this.CreateGraphDataContextNode(localPath);
                }
            }

            m_MainPreviewData.Initialize();
        }

        public bool IsSubGraph => ShaderGraphAssetModel.IsSubGraph;
        public override bool CanBeSubgraph() => IsSubGraph;

        /// <summary>
        /// Tests the connection between two GraphData ports at the data level.
        /// </summary>
        /// <param name="src">Source port.</param>
        /// <param name="dst">Destination port.</param>
        /// <returns>True if the ports can be connected, false otherwise.</returns>
        bool TestConnection(GraphDataPortModel src, GraphDataPortModel dst)
        {
            // temporarily disable connections to ports of different types.
            if (src.PortDataType != dst.PortDataType)
                return false;

            return GraphHandler.TestConnection(dst.owner.graphDataName,
                dst.graphDataName, src.owner.graphDataName,
                src.graphDataName, RegistryInstance);
        }

        /// <summary>
        /// Tries to connect two GraphData ports at the data level.
        /// </summary>
        /// <param name="src">Source port.</param>
        /// <param name="dst">Destination port.</param>
        /// <returns>True if the connection was successful, false otherwise.</returns>
        public bool TryConnect(GraphDataPortModel src, GraphDataPortModel dst)
        {
            return GraphHandler.TryConnect(
                src.owner.graphDataName, src.graphDataName,
                dst.owner.graphDataName, dst.graphDataName,
                RegistryInstance);
        }

        static bool PortsFormCycle(IPortModel fromPort, IPortModel toPort)
        {
            var queue = new Queue<IPortNodeModel>();
            queue.Enqueue(fromPort.NodeModel);

            while (queue.Count > 0)
            {
                var checkNode = queue.Dequeue();

                if (checkNode == toPort.NodeModel) return true;

                foreach (var incomingEdge in checkNode.GetIncomingEdges())
                {
                    queue.Enqueue(incomingEdge.FromPort.NodeModel);
                }
            }

            return false;
        }

        protected override bool IsCompatiblePort(IPortModel startPortModel, IPortModel compatiblePortModel)
        {
            if (startPortModel.Direction == compatiblePortModel.Direction) return false;

            var fromPort = startPortModel.Direction == PortDirection.Output ? startPortModel : compatiblePortModel;
            var toPort = startPortModel.Direction == PortDirection.Input ? startPortModel : compatiblePortModel;

            if (PortsFormCycle(fromPort, toPort)) return false;

            if (fromPort.NodeModel is RedirectNodeModel fromRedirect)
            {
                fromPort = fromRedirect.ResolveSource();
                if (fromPort == null) return true;
            }

            if (toPort.NodeModel is RedirectNodeModel toRedirect)
            {
                // Only connect to a hanging branch if it's valid for every connection.
                // Should not recurse more than once. ResolveDestinations returns non-redirect nodes.
                return toRedirect.ResolveDestinations().All(testPort => IsCompatiblePort(fromPort, testPort));
            }

            if ((fromPort, toPort) is (GraphDataPortModel fromDataPort, GraphDataPortModel toDataPort))
            {
                return fromDataPort.owner.existsInGraphData &&
                    toDataPort.owner.existsInGraphData &&
                    TestConnection(fromDataPort, toDataPort);
            }

            // Don't support connecting GraphDelta-backed ports to UI-only ones.
            if (fromPort is GraphDataPortModel || toPort is GraphDataPortModel)
            {
                return false;
            }

            return base.IsCompatiblePort(startPortModel, compatiblePortModel);
        }

        public IEnumerable<IVariableDeclarationModel> GetGraphProperties()
        {
            return this.VariableDeclarations;
        }

        public void GetTimeDependentNodesOnGraph(PooledHashSet<GraphDataNodeModel> timeDependentNodes)
        {
            var nodesOnGraph = NodeModels;
            foreach (var nodeModel in nodesOnGraph)
            {
                if (nodeModel is GraphDataNodeModel graphDataNodeModel && DoesNodeRequireTime(graphDataNodeModel))
                    timeDependentNodes.Add(graphDataNodeModel);
            }

            PropagateNodes(timeDependentNodes, PropagationDirection.Downstream, timeDependentNodes);
        }

        // cache the Action to avoid GC
        static readonly Action<GraphDataNodeModel> AddNextLevelNodesToWave =
            nextLevelNode =>
            {
                if (!m_TempAddedToNodeWave.Contains(nextLevelNode))
                {
                    m_TempNodeWave.Push(nextLevelNode);
                    m_TempAddedToNodeWave.Add(nextLevelNode);
                }
            };

        // Temp structures that are kept around statically to avoid GC churn (not thread safe)
        static Stack<GraphDataNodeModel> m_TempNodeWave = new();
        static HashSet<GraphDataNodeModel> m_TempAddedToNodeWave = new();

        // ADDs all nodes in sources, and all nodes in the given direction relative to them, into result
        // sources and result can be the same HashSet
        private static readonly ProfilerMarker PropagateNodesMarker = new ProfilerMarker("PropagateNodes");

        static void PropagateNodes(HashSet<GraphDataNodeModel> sources, PropagationDirection dir, HashSet<GraphDataNodeModel> result)
        {
            using (PropagateNodesMarker.Auto())
                if (sources.Count > 0)
                {
                    // NodeWave represents the list of nodes we still have to process and add to result
                    m_TempNodeWave.Clear();
                    m_TempAddedToNodeWave.Clear();
                    foreach (var node in sources)
                    {
                        m_TempNodeWave.Push(node);
                        m_TempAddedToNodeWave.Add(node);
                    }

                    while (m_TempNodeWave.Count > 0)
                    {
                        var node = m_TempNodeWave.Pop();
                        if (node == null)
                            continue;

                        result.Add(node);

                        // grab connected nodes in propagation direction, add them to the node wave
                        ForeachConnectedNode(node, dir, AddNextLevelNodesToWave);
                    }

                    // clean up any temp data
                    m_TempNodeWave.Clear();
                    m_TempAddedToNodeWave.Clear();
                }
        }

        static IEnumerable<NodeHandler> GetUpstreamNodes(NodeHandler startingNode)
        {
            return Utils.GraphTraversalUtils.GetUpstreamNodes(startingNode);
        }

        static IEnumerable<NodeHandler> GetDownstreamNodes(NodeHandler startingNode)
        {
            return Utils.GraphTraversalUtils.GetDownstreamNodes(startingNode);
        }

        static GraphDataNodeModel TryGetNodeModel(ShaderGraphModel shaderGraphModel, NodeHandler inputNodeReader)
        {
            // TODO: Make a mapping between every node model and node reader so we can also do lookup
            // from NodeReaders to NodeModels, as is needed below for instance
            return null;
        }

        static void ForeachConnectedNode(GraphDataNodeModel sourceNode, PropagationDirection dir, Action<GraphDataNodeModel> action)
        {
            sourceNode.TryGetNodeReader(out var nodeReader);

            ShaderGraphModel shaderGraphModel = sourceNode.GraphModel as ShaderGraphModel;

            // Enumerate through all nodes that the node feeds into and add them to the list of nodes to inspect
            if (dir == PropagationDirection.Downstream)
            {
                foreach (var connectedNode in GetDownstreamNodes(nodeReader))
                {
                    action(TryGetNodeModel(shaderGraphModel, connectedNode));
                }
            }
            else
            {
                foreach (var connectedNode in GetUpstreamNodes(nodeReader))
                {
                    action(TryGetNodeModel(shaderGraphModel, connectedNode));
                }
            }

            /* Custom Interpolator Blocks have implied connections to their Custom Interpolator Nodes...
            if (dir == PropagationDirection.Downstream && node is BlockNode bnode && bnode.isCustomBlock)
            {
                foreach (var cin in CustomInterpolatorUtils.GetCustomBlockNodeDependents(bnode))
                {
                    action(cin);
                }
            }
            // ... Just as custom Interpolator Nodes have implied connections to their custom interpolator blocks
            if (dir == PropagationDirection.Upstream && node is CustomInterpolatorNode ciNode && ciNode.e_targetBlockNode != null)
            {
                action(ciNode.e_targetBlockNode);
            } */
        }

        public IEnumerable<GraphDataNodeModel> GetNodesInHierarchyFromSources(IEnumerable<GraphDataNodeModel> nodeSources, PropagationDirection propagationDirection)
        {
            var nodesInHierarchy = new HashSet<GraphDataNodeModel>();
            PropagateNodes(new HashSet<GraphDataNodeModel>(nodeSources), propagationDirection, nodesInHierarchy);
            return nodesInHierarchy;
        }

        public static bool DoesNodeRequireTime(GraphDataNodeModel graphDataNodeModel)
        {
            bool nodeRequiresTime = false;
            if (graphDataNodeModel.TryGetNodeReader(out var _))
            {
                // TODO: Some way of making nodes be marked as requiring time or not
                // According to Esme, dependencies on globals/properties etc. will exist as RefNodes,
                // which are other INodeReaders/IPortReaders that exist as hidden/internal connections on a node
                //nodeReader.TryGetField("requiresTime", out var fieldReader);
                //if(fieldReader != null)
                //    fieldReader.TryGetValue(out nodeRequiresTime);
            }

            return nodeRequiresTime;
        }

        public static bool ShouldElementBeVisibleToSearcher(ShaderGraphModel shaderGraphModel, RegistryKey elementRegistryKey)
        {
            try
            {
                var nodeBuilder = shaderGraphModel.RegistryInstance.GetNodeBuilder(elementRegistryKey);
                var registryFlags = nodeBuilder.GetRegistryFlags();

                // commented out that bit cause it throws an exception for some elements at the moment
                return registryFlags.HasFlag(RegistryFlags.Func) /*|| registry.GetDefaultTopology(elementRegistryKey) == null*/;
            }
            catch (Exception exception)
            {
                AssertHelpers.Fail("Failed due to exception:" + exception);
                return false;
            }
        }

        public override IVariableNodeModel CreateVariableNode(IVariableDeclarationModel declarationModel,
            Vector2 position, SerializableGUID guid = default, SpawnFlags spawnFlags = SpawnFlags.Default)
        {
            Action<VariableNodeModel> initCallback = variableNodeModel =>
            {
                if (declarationModel is GraphDataVariableDeclarationModel model && variableNodeModel is GraphDataVariableNodeModel graphDataVariable)
                {
                    variableNodeModel.VariableDeclarationModel = model;

                    // Every time a variable node is added to the graph, add a reference node pointing back to the variable/property that is wrapped by the VariableDeclarationModel, on the CLDS level
                    GraphHandler.AddReferenceNode(guid.ToString(), model.contextNodeName, model.graphDataName, RegistryInstance);

                    // Currently using GTF guid of the variable node as its graph data name
                    graphDataVariable.graphDataName = guid.ToString();
                }
            };

            return this.CreateNode<GraphDataVariableNodeModel>(guid.ToString(), position, guid, initCallback, spawnFlags);
        }

        protected override Type GetDefaultVariableDeclarationType() => typeof(GraphDataVariableDeclarationModel);
    }
}
