using System;
using UnityEditor.ShaderGraph.GraphDelta;
using UnityEngine;
using Unity.GraphToolsFoundation;

namespace UnityEditor.ShaderGraph.GraphUI
{
    [Serializable]
    internal struct SamplerStateData
    {
        public SamplerStateType.Filter filter;
        public SamplerStateType.Wrap wrap;
        public bool depthCompare;
        public SamplerStateType.Aniso aniso;
    }

    [Serializable]
    class SamplerStateTypeConstant : BaseShaderGraphConstant
    {
        [SerializeField]
        SamplerStateData m_CopyPasteData;

        protected override object GetValue()
        {
            return new SamplerStateData {
            filter = SamplerStateType.GetFilter(GetField()),
            wrap = SamplerStateType.GetWrap(GetField()),
            depthCompare = SamplerStateType.GetDepthComparison(GetField()),
            aniso = SamplerStateType.GetAniso(GetField()) };
        }

        protected override void SetValue(object value)
        {
            var smp = (SamplerStateData)value;
            SamplerStateType.SetFilter(GetField(), smp.filter);
            SamplerStateType.SetWrap(GetField(), smp.wrap);
            SamplerStateType.SetDepthComparison(GetField(), smp.depthCompare);
            SamplerStateType.SetAniso(GetField(), smp.aniso);
    }

        public override object DefaultValue => new SamplerStateData();

        public override Type Type => typeof(SamplerStateData);

        public override TypeHandle GetTypeHandle() => ShaderGraphExampleTypes.SamplerStateTypeHandle;

        /// <inheritdoc />
        public override void OnBeforeCopy()
        {
            m_CopyPasteData = (SamplerStateData)ObjectValue;
        }

        /// <inheritdoc />
        public override void OnAfterPaste()
        {
            ObjectValue = m_CopyPasteData;
            m_CopyPasteData = default;
        }
    }
}
