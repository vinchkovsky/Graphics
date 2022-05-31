using System.Collections.Generic;
using System.Linq;
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

                    HandleNewModels(changeset);

                    HandleDeletedModels(changeset);

                    HandleChangedModels(changeset);
                }
            }
        }

        void HandleNewModels(GraphModelStateComponent.Changeset changeset)
        {
            var nodeModels = new List<GraphDataNodeModel>();
            var changedPortModel = new List<GraphDataPortModel>();

            foreach (var addedModel in changeset.NewModels)
            {
                switch (addedModel)
                {
                    case GraphDataNodeModel { HasPreview: true } graphDataNodeModel:
                    {
                        nodeModels.Add(graphDataNodeModel);
                        break;
                    }
                    case GraphDataEdgeModel { ToPort: GraphDataPortModel graphDataPortModel }:
                    {
                        changedPortModel.Add(graphDataPortModel);
                        break;
                    }
                }
            }

            foreach (var graphDataNodeModel in nodeModels)
            {
                m_PreviewManagerInstance.OnNodeAdded(graphDataNodeModel.graphDataName, graphDataNodeModel.Guid);
            }

            foreach (var graphDataPortModel in changedPortModel)
            {
                // Notify preview manager that this nodes connections have changed
                m_PreviewManagerInstance.OnNodeFlowChanged(graphDataPortModel.owner.graphDataName);
            }
        }

        void HandleDeletedModels(GraphModelStateComponent.Changeset changeset)
        {
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
                        if (graphDataEdgeModel.ToPort.NodeModel is GraphDataNodeModel graphDataNodeModel)
                            m_PreviewManagerInstance.OnNodeFlowChanged(graphDataNodeModel.graphDataName);
                        break;
                    }
                }
            }
        }

        void HandleChangedModels(GraphModelStateComponent.Changeset changeset)
        {
            foreach (var changedModel in changeset.ChangedModels)
            {
                switch (changedModel)
                {
                    case GraphDataVariableDeclarationModel variableDeclarationModel:
                        var variableConstant = variableDeclarationModel.InitializationModel as BaseShaderGraphConstant;
                        HandleConstantUpdate(variableConstant);
                        break;

                    case GraphDataPortModel graphDataPortModel:
                        var portConstant = graphDataPortModel.EmbeddedValue as BaseShaderGraphConstant;
                        // Possible when connecting a port gets an edge connection, constant isn't initialized yet
                        if (portConstant == null)
                            continue;
                        HandleConstantUpdate(portConstant);
                        break;
                }
            }
        }

        void HandleConstantUpdate(BaseShaderGraphConstant variableConstant)
        {
            if (variableConstant.NodeName == Registry.ResolveKey<PropertyContext>().Name)
                m_PreviewManagerInstance.OnGlobalPropertyChanged(variableConstant.PortName, variableConstant.ObjectValue);
            else
                m_PreviewManagerInstance.OnLocalPropertyChanged(variableConstant.NodeName, variableConstant.PortName, variableConstant.ObjectValue);
        }
    }
}
