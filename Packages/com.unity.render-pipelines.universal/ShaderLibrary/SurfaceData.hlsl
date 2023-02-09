#ifndef UNIVERSAL_SURFACE_DATA_INCLUDED
#define UNIVERSAL_SURFACE_DATA_INCLUDED

// Must match Universal ShaderGraph master node
struct SurfaceData
{
    half3 albedo;

    half3 specular;
    half  metallic;
    half  smoothness;

    half3 specular2;
    half  metallic2;
    half  smoothness2;

    half3 normalTS;
    half3 normalTS2;

    half3 emission;
    half  occlusion;
    half  alpha;
    half  clearCoatMask;
    half  clearCoatSmoothness;
};

#endif
