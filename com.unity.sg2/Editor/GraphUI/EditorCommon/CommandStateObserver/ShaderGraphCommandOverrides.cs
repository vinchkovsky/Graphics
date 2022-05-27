using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.ShaderGraph.GraphDelta;

namespace UnityEditor.ShaderGraph.GraphUI
{
    public static class ShaderGraphCommandOverrides
    {
        public static void HandleGraphElementRenamed(
            GraphViewStateComponent graphViewState,
            PreviewManager previewManager,
            RenameElementCommand renameElementCommand
        )
        {
            // TODO: Handle Properties being renamed when those come online
            //if (renameElementCommand.Model is IVariableDeclarationModel variableDeclarationModel)
            //{
            //    // React to property being renamed by finding all linked property nodes and marking them as requiring recompile and also needing constant value update
            //    var graphNodes = graphViewState.GraphModel.NodeModels;
            //    foreach (var graphNode in graphNodes)
            //    {
            //        if (graphNode is IVariableNodeModel variableNodeModel && Equals(variableNodeModel.VariableDeclarationModel, variableDeclarationModel))
            //        {
            //            previewManager.NotifyNodeFlowChanged(variableNodeModel);
            //        }
            //    }
            //}
        }

        public static void HandleUpdateConstantValue(
            UndoStateComponent undoState,
            GraphModelStateComponent graphModelState,
            PreviewManager previewManager,
            UpdateConstantValueCommand updateConstantValueCommand)
        {
            var shaderGraphModel = (ShaderGraphModel)graphModelState.GraphModel;
            if (updateConstantValueCommand.Constant is not BaseShaderGraphConstant cldsConstant) return;

            if (cldsConstant.NodeName == Registry.ResolveKey<PropertyContext>().Name)
            {
                previewManager.OnGlobalPropertyChanged(cldsConstant.PortName, updateConstantValueCommand.Value);
                return;
            }

            var nodeWriter = shaderGraphModel.GraphHandler.GetNode(cldsConstant.NodeName);
            if (nodeWriter != null)
            {
                previewManager.OnLocalPropertyChanged(cldsConstant.NodeName, cldsConstant.PortName, updateConstantValueCommand.Value);
            }
        }
    }
}
