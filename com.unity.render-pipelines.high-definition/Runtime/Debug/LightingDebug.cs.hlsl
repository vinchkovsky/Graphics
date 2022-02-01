//
// This file was automatically generated. Please don't edit by hand. Execute Editor command [ Edit / Render Pipeline / Generate Shader Includes ] instead
//

#ifndef LIGHTINGDEBUG_CS_HLSL
#define LIGHTINGDEBUG_CS_HLSL
//
// UnityEngine.Rendering.HighDefinition.DebugLightingMode:  static fields
//
#define DEBUGLIGHTINGMODE_NONE (0)
#define DEBUGLIGHTINGMODE_DIFFUSE_LIGHTING (1)
#define DEBUGLIGHTINGMODE_SPECULAR_LIGHTING (2)
#define DEBUGLIGHTINGMODE_DIRECT_DIFFUSE_LIGHTING (3)
#define DEBUGLIGHTINGMODE_DIRECT_SPECULAR_LIGHTING (4)
#define DEBUGLIGHTINGMODE_INDIRECT_DIFFUSE_LIGHTING (5)
#define DEBUGLIGHTINGMODE_REFLECTION_LIGHTING (6)
#define DEBUGLIGHTINGMODE_REFRACTION_LIGHTING (7)
#define DEBUGLIGHTINGMODE_EMISSIVE_LIGHTING (8)
#define DEBUGLIGHTINGMODE_LUX_METER (9)
#define DEBUGLIGHTINGMODE_LUMINANCE_METER (10)
#define DEBUGLIGHTINGMODE_MATCAP_VIEW (11)
#define DEBUGLIGHTINGMODE_VISUALIZE_CASCADE (12)
#define DEBUGLIGHTINGMODE_VISUALIZE_SHADOW_MASKS (13)
#define DEBUGLIGHTINGMODE_INDIRECT_DIFFUSE_OCCLUSION (14)
#define DEBUGLIGHTINGMODE_INDIRECT_SPECULAR_OCCLUSION (15)
#define DEBUGLIGHTINGMODE_PROBE_VOLUME (16)

//
// UnityEngine.Rendering.HighDefinition.DebugLightFilterMode:  static fields
//
#define DEBUGLIGHTFILTERMODE_NONE (0)
#define DEBUGLIGHTFILTERMODE_DIRECT_DIRECTIONAL (1)
#define DEBUGLIGHTFILTERMODE_DIRECT_PUNCTUAL (2)
#define DEBUGLIGHTFILTERMODE_DIRECT_RECTANGLE (4)
#define DEBUGLIGHTFILTERMODE_DIRECT_TUBE (8)
#define DEBUGLIGHTFILTERMODE_DIRECT_SPOT_CONE (16)
#define DEBUGLIGHTFILTERMODE_DIRECT_SPOT_PYRAMID (32)
#define DEBUGLIGHTFILTERMODE_DIRECT_SPOT_BOX (64)
#define DEBUGLIGHTFILTERMODE_INDIRECT_REFLECTION_PROBE (128)
#define DEBUGLIGHTFILTERMODE_INDIRECT_PLANAR_PROBE (256)

//
// UnityEngine.Rendering.HighDefinition.DebugLightLayersMask:  static fields
//
#define DEBUGLIGHTLAYERSMASK_NONE (0)
#define DEBUGLIGHTLAYERSMASK_LIGHT_LAYER1 (1)
#define DEBUGLIGHTLAYERSMASK_LIGHT_LAYER2 (2)
#define DEBUGLIGHTLAYERSMASK_LIGHT_LAYER3 (4)
#define DEBUGLIGHTLAYERSMASK_LIGHT_LAYER4 (8)
#define DEBUGLIGHTLAYERSMASK_LIGHT_LAYER5 (16)
#define DEBUGLIGHTLAYERSMASK_LIGHT_LAYER6 (32)
#define DEBUGLIGHTLAYERSMASK_LIGHT_LAYER7 (64)
#define DEBUGLIGHTLAYERSMASK_LIGHT_LAYER8 (128)

//
// UnityEngine.Rendering.HighDefinition.ShadowMapDebugMode:  static fields
//
#define SHADOWMAPDEBUGMODE_NONE (0)
#define SHADOWMAPDEBUGMODE_VISUALIZE_PUNCTUAL_LIGHT_ATLAS (1)
#define SHADOWMAPDEBUGMODE_VISUALIZE_DIRECTIONAL_LIGHT_ATLAS (2)
#define SHADOWMAPDEBUGMODE_VISUALIZE_AREA_LIGHT_ATLAS (3)
#define SHADOWMAPDEBUGMODE_VISUALIZE_CACHED_PUNCTUAL_LIGHT_ATLAS (4)
#define SHADOWMAPDEBUGMODE_VISUALIZE_CACHED_AREA_LIGHT_ATLAS (5)
#define SHADOWMAPDEBUGMODE_VISUALIZE_SHADOW_MAP (6)
#define SHADOWMAPDEBUGMODE_SINGLE_SHADOW (7)

//
// UnityEngine.Rendering.HighDefinition.ExposureDebugMode:  static fields
//
#define EXPOSUREDEBUGMODE_NONE (0)
#define EXPOSUREDEBUGMODE_SCENE_EV100VALUES (1)
#define EXPOSUREDEBUGMODE_HISTOGRAM_VIEW (2)
#define EXPOSUREDEBUGMODE_FINAL_IMAGE_HISTOGRAM_VIEW (3)
#define EXPOSUREDEBUGMODE_METERING_WEIGHTED (4)

//
// UnityEngine.Rendering.HighDefinition.ProbeVolumeDebugMode:  static fields
//
#define PROBEVOLUMEDEBUGMODE_NONE (0)
#define PROBEVOLUMEDEBUGMODE_VISUALIZE_ATLAS (1)
#define PROBEVOLUMEDEBUGMODE_VISUALIZE_DEBUG_COLORS (2)
#define PROBEVOLUMEDEBUGMODE_VISUALIZE_VALIDITY (3)

//
// UnityEngine.Rendering.HighDefinition.ProbeVolumeAtlasSliceMode:  static fields
//
#define PROBEVOLUMEATLASSLICEMODE_IRRADIANCE_SH00 (0)
#define PROBEVOLUMEATLASSLICEMODE_IRRADIANCE_SH1_1 (1)
#define PROBEVOLUMEATLASSLICEMODE_IRRADIANCE_SH10 (2)
#define PROBEVOLUMEATLASSLICEMODE_IRRADIANCE_SH11 (3)
#define PROBEVOLUMEATLASSLICEMODE_IRRADIANCE_SH2_2 (4)
#define PROBEVOLUMEATLASSLICEMODE_IRRADIANCE_SH2_1 (5)
#define PROBEVOLUMEATLASSLICEMODE_IRRADIANCE_SH20 (6)
#define PROBEVOLUMEATLASSLICEMODE_IRRADIANCE_SH21 (7)
#define PROBEVOLUMEATLASSLICEMODE_IRRADIANCE_SH22 (8)
#define PROBEVOLUMEATLASSLICEMODE_VALIDITY (9)
#define PROBEVOLUMEATLASSLICEMODE_OCTAHEDRAL_DEPTH (10)

//
// UnityEngine.Rendering.HighDefinition.MaskVolumeDebugMode:  static fields
//
#define MASKVOLUMEDEBUGMODE_NONE (0)
#define MASKVOLUMEDEBUGMODE_VISUALIZE_ATLAS (1)


#endif
