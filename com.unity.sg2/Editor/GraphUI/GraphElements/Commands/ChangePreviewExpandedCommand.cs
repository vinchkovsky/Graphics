using System.Collections.Generic;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;
using UnityEngine.GraphToolsFoundation.CommandStateObserver;

namespace UnityEditor.ShaderGraph.GraphUI
{
    public class ChangePreviewExpandedCommand : ModelCommand<IGraphElementModel>
    {
        bool m_IsPreviewExpanded;
        public ChangePreviewExpandedCommand(bool isPreviewExpanded, IReadOnlyList<IGraphElementModel> models)
            : base("Change Preview Expansion", "Change Previews Expansion", models)
        {
            m_IsPreviewExpanded = isPreviewExpanded;
        }

        public static void DefaultCommandHandler(
            UndoStateComponent undoState,
            GraphModelStateComponent graphModelState,
            PreviewManager previewManager,
            ChangePreviewExpandedCommand command
        )
        {
            using (var undoStateUpdater = undoState.UpdateScope)
            {
                undoStateUpdater.SaveSingleState(graphModelState, command);
            }

            using (var graphUpdater = graphModelState.UpdateScope)
            {
                foreach (var graphElementModel in command.Models)
                {
                    if(graphElementModel is GraphDataNodeModel graphDataNodeModel)
                        graphDataNodeModel.IsPreviewExpanded = command.m_IsPreviewExpanded;
                    graphUpdater.MarkChanged(command.Models, ChangeHint.Layout);
                }
            }
        }
    }
}
