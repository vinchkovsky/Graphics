using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.CommandStateObserver;

namespace UnityEditor.ShaderGraph.GraphUI
{
    public static class ShaderGraphCommandsRegistrar
    {
        public static void RegisterCommandHandlers(BaseGraphTool graphTool, GraphView graphView, GraphViewModel graphViewModel, PreviewManager previewManager, ShaderGraphModel shaderGraphModel, Dispatcher dispatcher)
        {
            if (dispatcher is not CommandDispatcher commandDispatcher)
                return;

            // Shader Graph commands
            commandDispatcher.RegisterCommandHandler<UndoStateComponent, GraphModelStateComponent, AddRedirectNodeCommand>(
                AddRedirectNodeCommand.DefaultHandler,
                graphTool.UndoStateComponent,
                graphViewModel.GraphModelState);

            commandDispatcher.RegisterCommandHandler<UndoStateComponent, GraphModelStateComponent, PreviewManager, ChangePreviewExpandedCommand>(
                ChangePreviewExpandedCommand.DefaultCommandHandler,
                graphTool.UndoStateComponent,
                graphViewModel.GraphModelState,
                previewManager
            );

            commandDispatcher.RegisterCommandHandler<UndoStateComponent, ShaderGraphAssetModel, ChangeActiveTargetsCommand>(
                ChangeActiveTargetsCommand.DefaultCommandHandler,
                graphTool.UndoStateComponent,
                shaderGraphModel.ShaderGraphAssetModel
            );

            commandDispatcher.RegisterCommandHandler<UndoStateComponent, ShaderGraphAssetModel, ChangeTargetSettingsCommand>(
                ChangeTargetSettingsCommand.DefaultCommandHandler,
                graphTool.UndoStateComponent,
                shaderGraphModel.ShaderGraphAssetModel
            );

            //commandDispatcher.RegisterCommandHandler<UndoStateComponent, GraphViewStateComponent, PreviewManager, ChangePreviewModeCommand>(
            //    ChangePreviewModeCommand.DefaultCommandHandler,
            //    graphTool.UndoStateComponent,
            //    graphView.GraphViewState,
            //    previewManager
            //);

            // Overrides for default GTF commands
            dispatcher.RegisterCommandHandler<BaseGraphTool, GraphViewModel, PreviewManager, CreateEdgeCommand>(
                ShaderGraphCommandOverrides.HandleCreateEdge,
                graphTool,
                graphViewModel,
                previewManager);

            // Unregister the base GraphView command handling for this as we want to insert our own
            graphView.Dispatcher.UnregisterCommandHandler<DeleteElementsCommand>();
            dispatcher.RegisterCommandHandler<UndoStateComponent, GraphModelStateComponent, SelectionStateComponent, PreviewManager, DeleteElementsCommand>(
                ShaderGraphCommandOverrides.HandleDeleteElements,
                graphTool.UndoStateComponent,
                graphViewModel.GraphModelState,
                graphViewModel.SelectionState,
                previewManager);

            dispatcher.RegisterCommandHandler<UndoStateComponent, GraphModelStateComponent, SelectionStateComponent, PreviewManager, BypassNodesCommand>(
                ShaderGraphCommandOverrides.HandleBypassNodes,
                graphTool.UndoStateComponent,
                graphViewModel.GraphModelState,
                graphViewModel.SelectionState,
                previewManager);

            dispatcher.RegisterCommandHandler<GraphViewStateComponent, PreviewManager, RenameElementCommand>(
                ShaderGraphCommandOverrides.HandleGraphElementRenamed,
                graphViewModel.GraphViewState,
                previewManager);

            dispatcher.RegisterCommandHandler<UndoStateComponent, GraphModelStateComponent, PreviewManager, UpdateConstantValueCommand>(
                ShaderGraphCommandOverrides.HandleUpdateConstantValue,
                graphTool.UndoStateComponent,
                graphViewModel.GraphModelState,
                previewManager);

            // Node UI commands
            dispatcher.RegisterCommandHandler<UndoStateComponent, GraphModelStateComponent, PreviewManager, SetGraphTypeValueCommand>(
                SetGraphTypeValueCommand.DefaultCommandHandler,
                graphTool.UndoStateComponent,
                graphViewModel.GraphModelState,
                previewManager);

            dispatcher.RegisterCommandHandler<UndoStateComponent, GraphModelStateComponent, PreviewManager, SetGradientTypeValueCommand>(
                SetGradientTypeValueCommand.DefaultCommandHandler,
                graphTool.UndoStateComponent,
                graphViewModel.GraphModelState,
                previewManager);

            dispatcher.RegisterCommandHandler<UndoStateComponent, GraphModelStateComponent, ChangeNodeFunctionCommand>(
                ChangeNodeFunctionCommand.DefaultCommandHandler,
                graphTool.UndoStateComponent,
                graphViewModel.GraphModelState);
        }
    }
}
