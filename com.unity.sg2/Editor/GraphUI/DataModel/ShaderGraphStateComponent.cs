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

        public string graphModelJson => m_GraphJSON;

        public string targetSettingsJson => m_TargetSettingsJSON;

        [NonSerialized]
        ShaderGraphAssetModel m_AssetModel;

        public ShaderGraphStateComponent(ShaderGraphAssetModel assetModel)
        {
            m_AssetModel = assetModel;
            m_GraphJSON = assetModel.GraphHandler.ToSerializedFormat();
            m_TargetSettingsJSON =  MultiJson.Serialize(assetModel.targetSettingsObject);
        }

        public class StateUpdater : BaseUpdater<ShaderGraphStateComponent>
        {
            public void UpdateShaderGraphState()
            {
                if(m_State.m_AssetModel == null)
                    return;

                m_State.m_GraphJSON = m_State.m_AssetModel.GraphHandler.ToSerializedFormat();
                m_State.m_TargetSettingsJSON =  MultiJson.Serialize(m_State.m_AssetModel.targetSettingsObject);
            }
        }
    }
}
