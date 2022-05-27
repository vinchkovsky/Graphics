using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.ShaderGraph.GraphDelta;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.CommandStateObserver;

namespace UnityEditor.ShaderGraph.GraphUI
{
    public class GraphModelStateObserver : StateObserver
    {
        PreviewManager m_PreviewManagerInstance;
        GraphModelStateComponent m_GraphModelStateComponent;
        GraphHandler m_GraphHandler;

        public GraphModelStateObserver(
            GraphModelStateComponent graphModelStateComponent,
            PreviewManager previewManager,
            GraphHandler graphHandler) : base(new IStateComponent[] {graphModelStateComponent}, new IStateComponent[] {graphModelStateComponent})
        {
            m_PreviewManagerInstance = previewManager;
            m_GraphModelStateComponent = graphModelStateComponent;
            m_GraphHandler = graphHandler;
        }

        public override void Observe()
        {
            // Note: These using statements are necessary to increment last observed version
            using (var graphViewObservation = this.ObserveState(m_GraphModelStateComponent))
            {
                if (graphViewObservation.UpdateType != UpdateType.None)
                {
                    var changeset = m_GraphModelStateComponent.GetAggregatedChangeset(graphViewObservation.LastObservedVersion);

                    foreach (var addedModel in changeset.NewModels)
                    {
                        switch (addedModel)
                        {
                            case GraphDataNodeModel { HasPreview: true } graphDataNodeModel:
                            {
                                m_PreviewManagerInstance.OnNodeAdded(graphDataNodeModel.graphDataName, graphDataNodeModel.Guid);
                                using var graphUpdater = m_GraphModelStateComponent.UpdateScope;
                                graphUpdater.MarkChanged(addedModel);
                                break;
                            }
                            case GraphDataEdgeModel { ToPort: GraphDataPortModel graphDataPortModel }:
                            {
                                // Notify preview manager that this nodes connections have changed
                                m_PreviewManagerInstance.OnNodeFlowChanged(graphDataPortModel.owner.graphDataName);
                                break;
                            }
                        }
                    }

                    foreach (var removedModel in changeset.DeletedModels)
                    {
                        switch (removedModel)
                        {
                            case GraphDataNodeModel graphDataNodeModel:
                            {
                                m_PreviewManagerInstance.OnNodeRemoved(graphDataNodeModel.graphDataName);
                                // TODO: It'd be nice to keep this in the shader graph model if possible
                                m_GraphHandler.RemoveNode(graphDataNodeModel.graphDataName);
                                break;
                            }
                            case GraphDataEdgeModel graphDataEdgeModel:
                            {
                                if(graphDataEdgeModel.ToPort.NodeModel is GraphDataNodeModel graphDataNodeModel)
                                    m_PreviewManagerInstance.OnNodeFlowChanged(graphDataNodeModel.graphDataName);
                                break;
                            }
                        }
                    }

                    foreach (var modelAndChangeHintPair in changeset.ChangedModelsAndHints)
                    {
                        switch (modelAndChangeHintPair.Key)
                        {
                            case GraphDataVariableDeclarationModel variableDeclarationModel :
                                var cldsConstant = variableDeclarationModel.InitializationModel as BaseShaderGraphConstant;
                                if (cldsConstant.NodeName == Registry.ResolveKey<PropertyContext>().Name)
                                    m_PreviewManagerInstance.OnGlobalPropertyChanged(cldsConstant.PortName, cldsConstant.ObjectValue);
                                else
                                    m_PreviewManagerInstance.OnLocalPropertyChanged(cldsConstant.NodeName, cldsConstant.PortName, cldsConstant.ObjectValue);
                                break;
                        }
                    }
                }
            }
        }
    }
}
