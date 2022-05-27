using System.Collections.Generic;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.CommandStateObserver;

namespace UnityEditor.ShaderGraph.GraphUI
{
    class ChangeActiveTargetsCommand : UndoableCommand
    {
        public ChangeActiveTargetsCommand()
        {
            UndoString = "Change Active Targets";
        }

        public static void DefaultCommandHandler(
            UndoStateComponent undoState,
            GraphModelStateComponent graphModelState,
            ShaderGraphAssetModel graphAsset,
            ChangeActiveTargetsCommand command)
        {
            Debug.Log("ChangeActiveTargetsCommand: Target Settings Change is unimplemented");

            graphAsset.MarkAsDirty(true);

            // TODO: Consequences of adding a target: Discovering any new context node ports, validating all nodes on the graph etc.
        }
    }
}
