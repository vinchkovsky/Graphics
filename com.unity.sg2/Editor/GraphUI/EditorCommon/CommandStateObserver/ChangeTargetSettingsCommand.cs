using UnityEngine;
using UnityEditor.GraphToolsFoundation.Overdrive;
using System.Linq;
using UnityEngine.GraphToolsFoundation.CommandStateObserver;

namespace UnityEditor.ShaderGraph.GraphUI
{
    class ChangeTargetSettingsCommand : UndoableCommand
    {
        public ChangeTargetSettingsCommand()
        {
        }

        public static void DefaultCommandHandler(
            UndoStateComponent undoState,
            GraphModelStateComponent graphModelState,
            PreviewUpdateDispatcher previewUpdateDispatcher,
            ChangeTargetSettingsCommand command)
        {
            using (var undoStateUpdater = undoState.UpdateScope)
            {
                undoStateUpdater.SaveSingleState(graphModelState , command);
            }

            Debug.Log("ChangeTargetSettingsCommand: Target Settings Change is unimplemented");

            var shaderGraphModel = graphModelState.GraphModel as ShaderGraphModel;
            foreach (var target in shaderGraphModel.Targets)
            {
                shaderGraphModel.InitializeContextFromTarget(target);
            }

            previewUpdateDispatcher.OnListenerConnectionChanged(ShaderGraphAssetUtils.kMainEntryContextName);
            using var graphUpdater = graphModelState.UpdateScope;
            graphUpdater.MarkChanged(shaderGraphModel.NodeModels.OfType<GraphDataContextNodeModel>());
            // TODO: Consequences of changing a target setting: Discovering any new context node ports, validating all nodes on the graph etc.
        }
    }
}
