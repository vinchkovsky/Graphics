using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.CommandStateObserver;

namespace UnityEditor.ShaderGraph.GraphUI
{
    public class ShaderGraphCommandDispatcher : CommandDispatcher
    {
        protected override void PostDispatchCommand(ICommand command)
        {
            base.PostDispatchCommand(command);

            if (command is UndoRedoCommand)
            {
            }
        }
    }
}
