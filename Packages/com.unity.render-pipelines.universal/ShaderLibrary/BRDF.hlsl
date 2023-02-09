#ifndef UNIVERSAL_BRDF_INCLUDED
#define UNIVERSAL_BRDF_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/BSDF.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Deprecated.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceData.hlsl"

#define kDielectricSpec half4(0.04, 0.04, 0.04, 1.0 - 0.04) // standard dielectric reflectivity coef at incident angle (= 4%)

struct BRDFData
{
    half3 albedo;
    half3 diffuse;
    half3 diffuse2;
    half3 specular;
    half3 specular2;
    half reflectivity;
    half reflectivity2;
    half perceptualRoughness;
    half perceptualRoughness2;

    half roughness;
    half roughness2;

    half roughness_2;
    half roughness2_2;

    half grazingTerm;
    half grazingTerm2;

    // We save some light invariant BRDF terms so we don't have to recompute
    // them in the light loop. Take a look at DirectBRDF function for detailed explaination.
    half normalizationTerm;     // roughness * 4.0 + 2.0
    half normalizationTerm2;     // roughness * 4.0 + 2.0
    half roughness2MinusOne;    // roughness^2 - 1.0
    half roughness2MinusOne2;    // roughness^2 - 1.0
};

half ReflectivitySpecular(half3 specular)
{
#if defined(SHADER_API_GLES)
    return specular.r; // Red channel - because most metals are either monochrome or with redish/yellowish tint
#else
    return Max3(specular.r, specular.g, specular.b);
#endif
}

half OneMinusReflectivityMetallic(half metallic)
{
    // We'll need oneMinusReflectivity, so
    //   1-reflectivity = 1-lerp(dielectricSpec, 1, metallic) = lerp(1-dielectricSpec, 0, metallic)
    // store (1-dielectricSpec) in kDielectricSpec.a, then
    //   1-reflectivity = lerp(alpha, 0, metallic) = alpha + metallic*(0 - alpha) =
    //                  = alpha - metallic * alpha
    half oneMinusDielectricSpec = kDielectricSpec.a;
    return oneMinusDielectricSpec - metallic * oneMinusDielectricSpec;
}

half MetallicFromReflectivity(half reflectivity)
{
    half oneMinusDielectricSpec = kDielectricSpec.a;
    return (reflectivity - kDielectricSpec.r) / oneMinusDielectricSpec;
}

inline void InitializeBRDFDataDirect(half3 albedo, half3 diffuse, half3 diffuse2, half3 specular, half3 specular2, half reflectivity, half reflectivity2, half oneMinusReflectivity, half oneMinusReflectivity2, half smoothness, half smoothness2, inout half alpha, out BRDFData outBRDFData)
{
    outBRDFData = (BRDFData)0;
    outBRDFData.albedo = albedo;
    outBRDFData.diffuse = diffuse;
    outBRDFData.diffuse2 = diffuse2;
    outBRDFData.specular = specular;
    outBRDFData.specular2 = specular2;
    outBRDFData.reflectivity = reflectivity;
    outBRDFData.reflectivity2 = reflectivity2;

    outBRDFData.perceptualRoughness = PerceptualSmoothnessToPerceptualRoughness(smoothness);
    outBRDFData.perceptualRoughness2 = PerceptualSmoothnessToPerceptualRoughness(smoothness2);

    outBRDFData.roughness           = max(PerceptualRoughnessToRoughness(outBRDFData.perceptualRoughness), HALF_MIN_SQRT);
    outBRDFData.roughness2          = max(outBRDFData.roughness * outBRDFData.roughness, HALF_MIN);

    outBRDFData.roughness_2 = max(PerceptualRoughnessToRoughness(outBRDFData.perceptualRoughness2), HALF_MIN_SQRT);
    outBRDFData.roughness2_2 = max(outBRDFData.roughness_2 * outBRDFData.roughness_2, HALF_MIN);

    outBRDFData.grazingTerm         = saturate(smoothness + reflectivity);
    outBRDFData.grazingTerm2 = saturate(smoothness2 + reflectivity2);

    outBRDFData.normalizationTerm   = outBRDFData.roughness * half(4.0) + half(2.0);
    outBRDFData.normalizationTerm2 = outBRDFData.roughness2 * half(4.0) + half(2.0);

    outBRDFData.roughness2MinusOne  = outBRDFData.roughness2 - half(1.0);
    outBRDFData.roughness2MinusOne2 = outBRDFData.roughness2_2 - half(1.0);

    // Input is expected to be non-alpha-premultiplied while ROP is set to pre-multiplied blend.
    // We use input color for specular, but (pre-)multiply the diffuse with alpha to complete the standard alpha blend equation.
    // In shader: Cs' = Cs * As, in ROP: Cs' + Cd(1-As);
    // i.e. we only alpha blend the diffuse part to background (transmittance).
    #if defined(_ALPHAPREMULTIPLY_ON)
        // TODO: would be clearer to multiply this once to accumulated diffuse lighting at end instead of the surface property.
        outBRDFData.diffuse *= alpha;
    #endif
}

// Legacy: do not call, will not correctly initialize albedo property.
inline void InitializeBRDFDataDirect(half3 diffuse, half3 diffuse2, half3 specular, half3 specular2, half reflectivity, half reflectivity2, half oneMinusReflectivity, half oneMinusReflectivity2, half smoothness, half smoothness2, inout half alpha, out BRDFData outBRDFData)
{
    InitializeBRDFDataDirect(half3(0.0, 0.0, 0.0), diffuse, diffuse2, specular, specular2, reflectivity, reflectivity2, oneMinusReflectivity, oneMinusReflectivity2, smoothness, smoothness2, alpha, outBRDFData);
}

// Initialize BRDFData for material, managing both specular and metallic setup using shader keyword _SPECULAR_SETUP.
inline void InitializeBRDFData(half3 albedo, half metallic, half metallic2, half3 specular, half3 specular2, half smoothness, half smoothness2, inout half alpha, out BRDFData outBRDFData)
{
#ifdef _SPECULAR_SETUP
    half reflectivity = ReflectivitySpecular(specular);
    half reflectivity2 = ReflectivitySpecular(specular2);

    half oneMinusReflectivity = half(1.0) - reflectivity;
    half oneMinusReflectivity2 = half(1.0) - reflectivity2;

    half3 brdfDiffuse = albedo * oneMinusReflectivity;
    half3 brdfDiffuse2 = albedo * oneMinusReflectivity2;

    half3 brdfSpecular = specular;
    half3 brdfSpecular2 = specular2;
#else
    half oneMinusReflectivity = OneMinusReflectivityMetallic(metallic);
    half oneMinusReflectivity2 = OneMinusReflectivityMetallic(metallic2);

    half reflectivity = half(1.0) - oneMinusReflectivity;
    half reflectivity2 = half(1.0) - oneMinusReflectivity2;

    half3 brdfDiffuse = albedo * oneMinusReflectivity;
    half3 brdfDiffuse2 = albedo * oneMinusReflectivity2;

    half3 brdfSpecular = lerp(kDieletricSpec.rgb, albedo, metallic);
    half3 brdfSpecular2 = lerp(kDieletricSpec.rgb, albedo, metallic2);
#endif

    InitializeBRDFDataDirect(albedo, brdfDiffuse, brdfDiffuse2, brdfSpecular, brdfSpecular2, reflectivity, reflectivity2, oneMinusReflectivity, oneMinusReflectivity2, smoothness, smoothness2, alpha, outBRDFData);
}

inline void InitializeBRDFData(inout SurfaceData surfaceData, out BRDFData brdfData)
{
    InitializeBRDFData(surfaceData.albedo, surfaceData.metallic, surfaceData.metallic2, surfaceData.specular, surfaceData.specular2, surfaceData.smoothness, surfaceData.smoothness2, surfaceData.alpha, brdfData);
}

half3 ConvertF0ForClearCoat15(half3 f0)
{
    return ConvertF0ForAirInterfaceToF0ForClearCoat15Fast(f0);
}

inline void InitializeBRDFDataClearCoat(half clearCoatMask, half clearCoatSmoothness, inout BRDFData baseBRDFData, out BRDFData outBRDFData)
{
    outBRDFData = (BRDFData)0;
    outBRDFData.albedo = half(1.0);

    // Calculate Roughness of Clear Coat layer
    outBRDFData.diffuse = kDielectricSpec.aaa; // 1 - kDielectricSpec
    outBRDFData.diffuse2 = outBRDFData.diffuse; // 1 - kDielectricSpec
    outBRDFData.specular = kDielectricSpec.rgb;
    outBRDFData.specular2 = outBRDFData.specular;
    outBRDFData.reflectivity = kDielectricSpec.r;
    outBRDFData.reflectivity2 = outBRDFData.reflectivity;

    outBRDFData.perceptualRoughness = PerceptualSmoothnessToPerceptualRoughness(clearCoatSmoothness);
    outBRDFData.perceptualRoughness2 = outBRDFData.perceptualRoughness;

    outBRDFData.roughness           = max(PerceptualRoughnessToRoughness(outBRDFData.perceptualRoughness), HALF_MIN_SQRT);
    outBRDFData.roughness_2 = outBRDFData.roughness;

    outBRDFData.roughness2          = max(outBRDFData.roughness * outBRDFData.roughness, HALF_MIN);
    outBRDFData.roughness2_2 = outBRDFData.roughness2;

    outBRDFData.normalizationTerm   = outBRDFData.roughness * half(4.0) + half(2.0);
    outBRDFData.normalizationTerm2 = outBRDFData.normalizationTerm;

    outBRDFData.roughness2MinusOne  = outBRDFData.roughness2 - half(1.0);
    outBRDFData.roughness2MinusOne2 = outBRDFData.roughness2MinusOne;

    outBRDFData.grazingTerm         = saturate(clearCoatSmoothness + kDielectricSpec.x);
    outBRDFData.grazingTerm2 = outBRDFData.grazingTerm;

    // Modify Roughness of base layer using coat IOR
    half ieta                        = lerp(1.0h, CLEAR_COAT_IETA, clearCoatMask);
    half coatRoughnessScale          = Sq(ieta);
    half sigma                       = RoughnessToVariance(PerceptualRoughnessToRoughness(baseBRDFData.perceptualRoughness));

    baseBRDFData.perceptualRoughness = RoughnessToPerceptualRoughness(VarianceToRoughness(sigma * coatRoughnessScale));
    baseBRDFData.perceptualRoughness2 = baseBRDFData.perceptualRoughness;

    // Recompute base material for new roughness, previous computation should be eliminated by the compiler (as it's unused)
    baseBRDFData.roughness          = max(PerceptualRoughnessToRoughness(baseBRDFData.perceptualRoughness), HALF_MIN_SQRT);
    baseBRDFData.roughness2 = baseBRDFData.roughness;

    baseBRDFData.roughness2         = max(baseBRDFData.roughness * baseBRDFData.roughness, HALF_MIN);
    baseBRDFData.roughness2_2 = baseBRDFData.roughness2;

    baseBRDFData.normalizationTerm  = baseBRDFData.roughness * 4.0h + 2.0h;
    baseBRDFData.normalizationTerm2 = baseBRDFData.normalizationTerm;

    baseBRDFData.roughness2MinusOne = baseBRDFData.roughness2 - 1.0h;
    baseBRDFData.roughness2MinusOne2 = baseBRDFData.roughness2MinusOne;

    // Darken/saturate base layer using coat to surface reflectance (vs. air to surface)
    baseBRDFData.specular = lerp(baseBRDFData.specular, ConvertF0ForClearCoat15(baseBRDFData.specular), clearCoatMask);
    baseBRDFData.specular2 = baseBRDFData.specular;
    // TODO: what about diffuse? at least in specular workflow diffuse should be recalculated as it directly depends on it.
}

BRDFData CreateClearCoatBRDFData(SurfaceData surfaceData, inout BRDFData brdfData)
{
    BRDFData brdfDataClearCoat = (BRDFData)0;

    #if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
    // base brdfData is modified here, rely on the compiler to eliminate dead computation by InitializeBRDFData()
    InitializeBRDFDataClearCoat(surfaceData.clearCoatMask, surfaceData.clearCoatSmoothness, brdfData, brdfDataClearCoat);
    #endif

    return brdfDataClearCoat;
}

// Computes the specular term for EnvironmentBRDF
half3 EnvironmentBRDFSpecular(BRDFData brdfData, half fresnelTerm)
{
    float surfaceReduction = 1.0 / (brdfData.roughness2 + 1.0);
    return half3(surfaceReduction * lerp(brdfData.specular, brdfData.grazingTerm, fresnelTerm));
}

half3 EnvironmentBRDF(BRDFData brdfData, half3 indirectDiffuse, half3 indirectSpecular, half fresnelTerm)
{
    half3 c = indirectDiffuse * brdfData.diffuse;
    c += indirectSpecular * EnvironmentBRDFSpecular(brdfData, fresnelTerm);
    return c;
}

// Environment BRDF without diffuse for clear coat
half3 EnvironmentBRDFClearCoat(BRDFData brdfData, half clearCoatMask, half3 indirectSpecular, half fresnelTerm)
{
    float surfaceReduction = 1.0 / (brdfData.roughness2 + 1.0);
    return indirectSpecular * EnvironmentBRDFSpecular(brdfData, fresnelTerm) * clearCoatMask;
}

// Computes the scalar specular term for Minimalist CookTorrance BRDF
// NOTE: needs to be multiplied with reflectance f0, i.e. specular color to complete
half DirectBRDFSpecular(BRDFData brdfData, half3 normalWS, half3 lightDirectionWS, half3 viewDirectionWS)
{
    float3 lightDirectionWSFloat3 = float3(lightDirectionWS);
    float3 halfDir = SafeNormalize(lightDirectionWSFloat3 + float3(viewDirectionWS));

    float NoH = saturate(dot(float3(normalWS), halfDir));
    half LoH = half(saturate(dot(lightDirectionWSFloat3, halfDir)));

    // GGX Distribution multiplied by combined approximation of Visibility and Fresnel
    // BRDFspec = (D * V * F) / 4.0
    // D = roughness^2 / ( NoH^2 * (roughness^2 - 1) + 1 )^2
    // V * F = 1.0 / ( LoH^2 * (roughness + 0.5) )
    // See "Optimizing PBR for Mobile" from Siggraph 2015 moving mobile graphics course
    // https://community.arm.com/events/1155

    // Final BRDFspec = roughness^2 / ( NoH^2 * (roughness^2 - 1) + 1 )^2 * (LoH^2 * (roughness + 0.5) * 4.0)
    // We further optimize a few light invariant terms
    // brdfData.normalizationTerm = (roughness + 0.5) * 4.0 rewritten as roughness * 4.0 + 2.0 to a fit a MAD.
    float d = NoH * NoH * brdfData.roughness2MinusOne + 1.00001f;

    half LoH2 = LoH * LoH;
    half specularTerm = brdfData.roughness2 / ((d * d) * max(0.1h, LoH2) * brdfData.normalizationTerm);

    // On platforms where half actually means something, the denominator has a risk of overflow
    // clamp below was added specifically to "fix" that, but dx compiler (we convert bytecode to metal/gles)
    // sees that specularTerm have only non-negative terms, so it skips max(0,..) in clamp (leaving only min(100,...))
#if REAL_IS_HALF
    specularTerm = specularTerm - HALF_MIN;
    specularTerm = clamp(specularTerm, 0.0, 100.0); // Prevent FP16 overflow on mobiles
#endif

    return specularTerm;
}

// Based on Minimalist CookTorrance BRDF
// Implementation is slightly different from original derivation: http://www.thetenthplanet.de/archives/255
//
// * NDF [Modified] GGX
// * Modified Kelemen and Szirmay-Kalos for Visibility term
// * Fresnel approximated with 1/LdotH
half3 DirectBDRF(BRDFData brdfData, half3 normalWS, half3 lightDirectionWS, half3 viewDirectionWS, bool specularHighlightsOff)
{
    // Can still do compile-time optimisation.
    // If no compile-time optimized, extra overhead if branch taken is around +2.5% on some untethered platforms, -10% if not taken.
    [branch] if (!specularHighlightsOff)
    {
        half specularTerm = DirectBRDFSpecular(brdfData, normalWS, lightDirectionWS, viewDirectionWS);
        half3 color = brdfData.diffuse + specularTerm * brdfData.specular;
        return color;
    }
    else
        return brdfData.diffuse;
}

// Based on Minimalist CookTorrance BRDF
// Implementation is slightly different from original derivation: http://www.thetenthplanet.de/archives/255
//
// * NDF [Modified] GGX
// * Modified Kelemen and Szirmay-Kalos for Visibility term
// * Fresnel approximated with 1/LdotH
half3 DirectBRDF(BRDFData brdfData, half3 normalWS, half3 lightDirectionWS, half3 viewDirectionWS)
{
#ifndef _SPECULARHIGHLIGHTS_OFF
    return brdfData.diffuse + DirectBRDFSpecular(brdfData, normalWS, lightDirectionWS, viewDirectionWS) * brdfData.specular;
#else
    return brdfData.diffuse;
#endif
}

#endif
