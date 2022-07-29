using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.CommandStateObserver;
using UnityEngine.UIElements;

namespace UnityEditor.ShaderGraph.GraphUI
{
    class TargetLabelPropertyField : Label
    {
        ICommandTarget m_Target;
        public TargetLabelPropertyField(ICommandTarget commandTarget)
        {
            m_Target = commandTarget;
            RegisterCallback<MouseUpEvent>(OnRightClick);
        }

        void OnRightClick(MouseUpEvent evt)
        {
            if (evt.button == 2)
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Set as Active Target"), true, OnSetActiveTarget, text);
                // Create context menu to set as active target
            }
        }

        void OnSetActiveTarget(object targetName)
        {
            var targets = TargetSettingsInspector.GetTargets();
            foreach (var target in targets)
            {
                if(target.displayName == (string)targetName)
                    m_Target.Dispatch(new ChangeActiveTargetsCommand());

            }
        }
    }
}
