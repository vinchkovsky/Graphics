Shader "Hidden/HDRP/ProbeVolumeSamplingDebug"
{
    SubShader
    {
        Tags{ "RenderPipeline" = "HDRenderPipeline" "RenderType" = "Opaque" }

        LOD 100

        HLSLINCLUDE
        #pragma editor_sync_compilation
        #pragma target 4.5
        #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch
        #pragma multi_compile_fragment PROBE_VOLUMES_OFF PROBE_VOLUMES_L1 PROBE_VOLUMES_L2

        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/EntityLighting.hlsl"
        #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

        #include "Packages/com.unity.render-pipelines.core/Runtime/Debug/ProbeVolumeDebugBase.hlsl"

        #define PROBE_VOLUME_DEBUG_FUNCTION_SAMPLING
        #include "Packages/com.unity.render-pipelines.core/Runtime/Debug/ProbeVolumeDebugFunctions.hlsl"
        ENDHLSL

        Pass
        {
            Name "ForwardOnly"
            Tags { "LightMode" = "ForwardOnly" }

            ZTest LEqual
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            ENDHLSL
        }
    }
}
