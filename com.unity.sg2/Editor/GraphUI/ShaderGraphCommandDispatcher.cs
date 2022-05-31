using System;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.CommandStateObserver;

namespace UnityEditor.ShaderGraph.GraphUI
{
    public class ShaderGraphCommandDispatcher : CommandDispatcher
    {
        BaseGraphTool m_GraphTool;
        ShaderGraphModel shaderGraphModel => m_GraphTool.ToolState.GraphModel as ShaderGraphModel;

        /// <summary>
        /// Subscribers are notified of when the graph asset has been loaded
        /// </summary>
        public Action<ShaderGraphModel> OnGraphLoaded;

        /// <summary>
        /// Subscribers are notified of when an undo redo action has taken place
        /// </summary>
        public Action<ShaderGraphModel> OnUndoRedo;

        public ShaderGraphCommandDispatcher(BaseGraphTool graphTool)
        {
            m_GraphTool = graphTool;
        }

        ICommand m_LastCommand;

        public override void Dispatch(ICommand command, Diagnostics diagnosticFlags = Diagnostics.None)
        {
            m_LastCommand = command;

            base.Dispatch(command, diagnosticFlags);

            PostCommandHandling(m_LastCommand);
            m_LastCommand = null;
        }

        void PostCommandHandling(ICommand command)
        {
            switch (command)
            {
                case LoadGraphCommand:
                        OnGraphLoaded(shaderGraphModel);
                        break;
                    case UndoRedoCommand:
                        OnUndoRedo(shaderGraphModel);
                        break;
            }
        }
    }
}
