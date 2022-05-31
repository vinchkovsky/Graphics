using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.CommandStateObserver;

namespace UnityEditor.ShaderGraph.GraphUI
{
    public static class ShaderGraphCommandsRegistrar
    {
        public static void RegisterCommandHandlers(
            BaseGraphTool graphTool,
            GraphModelStateComponent graphModelStateComponent,
            PreviewManager previewManager,
            ShaderGraphModel shaderGraphModel,
            Dispatcher dispatcher)
        {
            if (dispatcher is not CommandDispatcher commandDispatcher)
                return;

            commandDispatcher.RegisterCommandHandler<UndoStateComponent, GraphModelStateComponent, AddRedirectNodeCommand>(
                AddRedirectNodeCommand.DefaultHandler,
                graphTool.UndoStateComponent,
                graphModelStateComponent);

            // Shader Graph commands
            commandDispatcher.RegisterCommandHandler<UndoStateComponent, GraphModelStateComponent, AddRedirectNodeCommand>(
                AddRedirectNodeCommand.DefaultHandler,
                graphTool.UndoStateComponent,
                graphModelStateComponent);

            commandDispatcher.RegisterCommandHandler<UndoStateComponent, GraphModelStateComponent, PreviewManager, ChangePreviewExpandedCommand>(
                ChangePreviewExpandedCommand.DefaultCommandHandler,
                graphTool.UndoStateComponent,
                graphModelStateComponent,
                previewManager
            );

            commandDispatcher.RegisterCommandHandler<ShaderGraphModel, PreviewManager, ChangePreviewMeshCommand>(
                ChangePreviewMeshCommand.DefaultCommandHandler,
                shaderGraphModel,
                previewManager
            );

            commandDispatcher.RegisterCommandHandler<UndoStateComponent, GraphModelStateComponent, PreviewManager, ChangePreviewModeCommand>(
                ChangePreviewModeCommand.DefaultCommandHandler,
                graphTool.UndoStateComponent,
                graphModelStateComponent,
                previewManager
            );

            commandDispatcher.RegisterCommandHandler<UndoStateComponent, GraphModelStateComponent, ShaderGraphAssetModel, ChangeActiveTargetsCommand>(
                ChangeActiveTargetsCommand.DefaultCommandHandler,
                graphTool.UndoStateComponent,
                graphModelStateComponent,
                shaderGraphModel.ShaderGraphAssetModel
            );

            commandDispatcher.RegisterCommandHandler<UndoStateComponent, GraphModelStateComponent, ShaderGraphAssetModel, ChangeTargetSettingsCommand>(
                ChangeTargetSettingsCommand.DefaultCommandHandler,
                graphTool.UndoStateComponent,
                graphModelStateComponent,
                shaderGraphModel.ShaderGraphAssetModel
            );

            //commandDispatcher.RegisterCommandHandler<UndoStateComponent, GraphViewStateComponent, PreviewManager, ChangePreviewModeCommand>(
            //    ChangePreviewModeCommand.DefaultCommandHandler,
            //    graphTool.UndoStateComponent,
            //    graphView.GraphViewState,
            //    previewManager
            //);

            // Node UI commands
            dispatcher.RegisterCommandHandler<UndoStateComponent, GraphModelStateComponent, PreviewManager, SetGraphTypeValueCommand>(
                SetGraphTypeValueCommand.DefaultCommandHandler,
                graphTool.UndoStateComponent,
                graphModelStateComponent,
                previewManager);

            dispatcher.RegisterCommandHandler<UndoStateComponent, GraphModelStateComponent, PreviewManager, SetGradientTypeValueCommand>(
                SetGradientTypeValueCommand.DefaultCommandHandler,
                graphTool.UndoStateComponent,
                graphModelStateComponent,
                previewManager);

            dispatcher.RegisterCommandHandler<UndoStateComponent, GraphModelStateComponent, ChangeNodeFunctionCommand>(
                ChangeNodeFunctionCommand.DefaultCommandHandler,
                graphTool.UndoStateComponent,
                graphModelStateComponent);

            // Node upgrade commands
            dispatcher.RegisterCommandHandler<UndoStateComponent, GraphModelStateComponent, DismissNodeUpgradeCommand>(
                DismissNodeUpgradeCommand.DefaultCommandHandler,
                graphTool.UndoStateComponent,
                graphModelStateComponent);

            dispatcher.RegisterCommandHandler<UndoStateComponent, GraphModelStateComponent, UpgradeNodeCommand>(
                UpgradeNodeCommand.DefaultCommandHandler,
                graphTool.UndoStateComponent,
                graphModelStateComponent);
        }
    }
}
