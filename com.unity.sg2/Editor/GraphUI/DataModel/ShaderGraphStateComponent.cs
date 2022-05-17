using System;
using UnityEditor.ShaderGraph.Serialization;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.CommandStateObserver;

namespace UnityEditor.ShaderGraph.GraphUI
{
    public class ShaderGraphStateComponent :  StateComponent<ShaderGraphStateComponent.StateUpdater>
    {
        [SerializeField]
        string m_GraphJSON = new (String.Empty);

        [SerializeField]
        string m_TargetSettingsJSON = new (String.Empty);

        [NonSerialized]
        ShaderGraphAssetModel m_AssetModel;

        public ShaderGraphStateComponent(ShaderGraphAssetModel assetModel)
        {
            m_AssetModel = assetModel;
        }

        public class StateUpdater : BaseUpdater<ShaderGraphStateComponent>
        {
            public void UpdateShaderGraphState()
            {
                m_State.m_GraphJSON = m_State.m_AssetModel.GraphHandler.ToSerializedFormat();
                m_State.m_TargetSettingsJSON =  MultiJson.Serialize(m_State.m_AssetModel.targetSettingsObject);
            }
        }
    }
}
