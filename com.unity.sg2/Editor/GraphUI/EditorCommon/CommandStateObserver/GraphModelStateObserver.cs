using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.CommandStateObserver;

namespace UnityEditor.ShaderGraph.GraphUI
{
    public class GraphModelStateObserver : StateObserver
    {
        PreviewManager m_PreviewManagerInstance;
        GraphModelStateComponent m_GraphModelStateComponent;

        public GraphModelStateObserver(
            GraphModelStateComponent graphModelStateComponent,
            PreviewManager previewManager) : base(new IStateComponent[] {graphModelStateComponent}, new IStateComponent[] {graphModelStateComponent})
        {
            m_PreviewManagerInstance = previewManager;
            m_GraphModelStateComponent = graphModelStateComponent;
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
                        if (addedModel is GraphDataNodeModel graphDataNodeModel && graphDataNodeModel.HasPreview)
                        {
                            m_PreviewManagerInstance.OnNodeAdded(graphDataNodeModel.graphDataName, graphDataNodeModel.Guid);
                            using var graphUpdater = m_GraphModelStateComponent.UpdateScope;
                            graphUpdater.MarkChanged(addedModel);
                        }
                    }

                    foreach (var addedModel in changeset.DeletedModels)
                    {
                        if (addedModel is GraphDataNodeModel graphDataNodeModel)
                        {
                            m_PreviewManagerInstance.OnNodeRemoved(graphDataNodeModel.graphDataName);
                        }
                    }
                }
            }
        }
    }
}
