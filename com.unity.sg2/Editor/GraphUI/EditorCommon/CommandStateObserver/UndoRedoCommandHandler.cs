using System.Linq;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.ShaderGraph.GraphDelta;
using UnityEditor.ShaderGraph.Serialization;
using UnityEngine.GraphToolsFoundation.CommandStateObserver;

namespace UnityEditor.ShaderGraph.GraphUI
{
    public static class UndoRedoCommandHandler
    {
        public static void HandleUndoRedo(
            IState stateComponentHolder,
            ShaderGraphAssetModel shaderGraphAssetModel,
            PreviewManager previewManager,
            UndoRedoCommand command)
        {
            var undoStateComponent = (UndoStateComponent)stateComponentHolder.AllStateComponents.First(component => component is UndoStateComponent);
            var graphModelStateComponent = (GraphModelStateComponent)stateComponentHolder.AllStateComponents.First(component => component is GraphModelStateComponent);
            var shaderGraphStateComponent = (ShaderGraphStateComponent)stateComponentHolder.AllStateComponents.First(component => component is ShaderGraphStateComponent);
            UndoRedoCommand.DefaultCommandHandler(undoStateComponent, command);




        }
    }
}
