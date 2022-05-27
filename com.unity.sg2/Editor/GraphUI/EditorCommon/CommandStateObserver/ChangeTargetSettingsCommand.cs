using System.Collections.Generic;
using UnityEngine;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine.GraphToolsFoundation.CommandStateObserver;

namespace UnityEditor.ShaderGraph.GraphUI
{
    class ChangeTargetSettingsCommand : UndoableCommand
    {
        public ChangeTargetSettingsCommand()
        {
            UndoString = "Target Settings changed";
        }

        public static void DefaultCommandHandler(
            UndoStateComponent undoState,
            GraphModelStateComponent graphModelState,
            ShaderGraphAssetModel graphAsset,
            ChangeTargetSettingsCommand command)
        {
            Debug.Log("ChangeTargetSettingsCommand: Target Settings Change is unimplemented");

            using (var undoStateUpdater = undoState.UpdateScope)
            {
                undoStateUpdater.SaveSingleState(graphModelState, command);
            }
            graphAsset.MarkAsDirty(true);

            // TODO: Consequences of changing a target setting: Discovering any new context node ports, validating all nodes on the graph etc.
        }
    }
}
