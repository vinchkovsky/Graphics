using System;
using System.Linq;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.CommandStateObserver;

namespace UnityEditor.ShaderGraph.GraphUI
{
    public class ShaderGraphCommandDispatcher : CommandDispatcher
    {
        Type[] m_CommandTypeWhitelist =
        {
            typeof(CreateNodeCommand),
            typeof(CreateEdgeCommand),
            typeof(DeleteElementsCommand)
        };

        int m_CurrentUndoGroup;


        ShaderGraphModel m_GraphModel;
        PreviewManager m_PreviewManager;

        protected override void PreDispatchCommand(ICommand command)
        {
            if(m_CommandTypeWhitelist.Contains(command.GetType()))
            {

            }

            base.PreDispatchCommand(command);
        }

        protected override void PostDispatchCommand(ICommand command)
        {
            if(command is UndoRedoCommand)
            {
                m_PreviewManager.HandleUndoRedo(m_GraphModel);
            }

            base.PostDispatchCommand(command);
        }
    }
}
