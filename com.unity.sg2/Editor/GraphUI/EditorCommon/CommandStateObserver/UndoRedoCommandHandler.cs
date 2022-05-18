using UnityEditor.GraphToolsFoundation.Overdrive;

namespace UnityEditor.ShaderGraph.GraphUI
{
    public static class UndoRedoCommandHandler
    {
        public static void HandleUndoRedo(
            UndoStateComponent undoStateComponent,
            GraphModelStateComponent graphModelStateComponent,
            ShaderGraphStateComponent shaderGraphStateComponent,
            PreviewManager previewManager,
            UndoRedoCommand command)
        {
            UndoRedoCommand.DefaultCommandHandler(undoStateComponent, command);

            previewManager.HandleUndoRedo(graphModelStateComponent.GraphModel);
        }
    }
}
