#ifndef AMBIENT_DICE
#define AMBIENT_DICE

struct AmbientDice
{
    float amplitude;
    float sharpness;
    float3 mean;
};

float AmbientDiceEvaluateFromDirection(in AmbientDice a, const in float3 direction)
{
    return a.amplitude * pow(saturate(dot(a.mean, direction)), a.sharpness);
}

struct AmbientDiceWrapped
{
    float amplitude;
    float sharpness;
    float3 mean;
};

float AmbientDiceWrappedEvaluateFromDirection(in AmbientDiceWrapped a, const in float3 direction)
{
    return a.amplitude * pow(saturate(dot(a.mean, direction) * 0.5 + 0.5), a.sharpness);
}

// Analytic function fit over the sharpness range of [2, 32]
// https://www.desmos.com/calculator/gl9lomqucs
float AmbientDiceIntegralFromSharpness(const in float sharpness)
{
    return exp2(6.14741 * pow(abs(sharpness * 12.5654 + 14.6469), -1.0)) * 18.5256 + -18.5238;
}

float AmbientDiceIntegral(const in AmbientDice a)
{
    return a.amplitude * AmbientDiceIntegralFromSharpness(a.sharpness);
}

// Does not include divide by PI required to normalize the clamped cosine diffuse BRDF.
// This was done to match the format of SGIrradianceFitted() which also does not include the divide by PI.
// Post dividing by PI is required.
float AmbientDiceAndClampedCosineProductIntegral(const in AmbientDice a, const in float3 clampedCosineNormal)
{
    float mDotN = dot(a.mean, clampedCosineNormal);

    float sharpnessScale = pow(abs(a.sharpness), -0.121796) * -2.18362 + 2.7562;
    float sharpnessBias = pow(abs(a.sharpness), 0.137288) * -0.555517 + 0.711175;

    mDotN = mDotN * sharpnessScale + sharpnessBias;

    float res = max(0.0, exp2(-9.45649 * pow(abs(mDotN * 0.240416 + 1.16513), -5.16291)) * 2.71745 + -0.00193676);
    res *= AmbientDiceIntegral(a);
    return res;
}


#if 1
// https://www.desmos.com/calculator/abirkxdgko
// Simple Least Squares Fit - raw data generated by directly projecting the ambient dice lobe to a zonal harmonic via integration.
// Likely contains ringing for some sharpness levels.
float ComputeZonalHarmonicC0FromAmbientDiceSharpness(float sharpness)
{
    // sharpness abs is to simply make the compiler happy.
    return pow(abs(sharpness) * 1.62301 + 1.59682, -0.993255) * 2.83522 + -0.001;
}

float ComputeZonalHarmonicC1FromAmbientDiceSharpness(float sharpness)
{
    return exp2(-3.56165 * pow(abs(sharpness * -7.42401 + -13.5339), -0.998517)) * -9.1717 + 9.1715;
}

float ComputeZonalHarmonicC2FromAmbientDiceSharpness(float sharpness)
{
    float lhs = 0.239989 + 0.42846 * sharpness + -0.202951 * sharpness * sharpness + 0.0303908 * sharpness * sharpness * sharpness;
    float rhs = exp2(-7.07205 * pow(abs(sharpness * 0.852362 + -0.469403), -0.898339)) * -0.557094 + 0.539681;
    return sharpness > 2.33 ? rhs : lhs;
}


// https://www.desmos.com/calculator/1ajnhdbg6j
// Simple Least Squares Fit - raw data generated by directly projecting the ambient dice lobe to a zonal harmonic via integration.
// Likely contains ringing for some sharpness levels.
float ComputeZonalHarmonicC0FromAmbientDiceWrappedSharpness(float sharpness)
{
    // sharpness abs is to simply make the compiler happy.
    return pow(abs(sharpness) * 0.816074 + 0.809515, -0.99663) * 2.87866 + -0.001;
}

float ComputeZonalHarmonicC1FromAmbientDiceWrappedSharpness(float sharpness)
{
    return exp2(-7.01231 * pow(abs(sharpness * 1.36435 + -1.3829), -0.786179)) * -1.14392 + 1.04683;
}

float ComputeZonalHarmonicC2FromAmbientDiceWrappedSharpness(float sharpness)
{
    float lhs = -0.438588 + 0.542959 * sharpness + -0.112098 * sharpness * sharpness + 0.00800693 * sharpness * sharpness * sharpness;
    float rhs = exp2(-6.39474 * pow(abs(sharpness * 0.488674 + -2.37692), -0.559034)) * -0.816382 + 0.473311;
    return sharpness > 5.0 ? rhs : lhs;
}


#else
// https://www.desmos.com/calculator/umjtgtzmk8
// Fit to pre-deringed data.
// The dering constraint is evaluated post diffuse brdf convolution.
// The signal can still ring in raw irradiance space.
float ComputeZonalHarmonicC0FromAmbientDiceSharpness(float sharpness)
{
    return pow(abs(sharpness * 1.62301 + 1.59682), -0.993255) * 2.83522 + -0.001;
}

float ComputeZonalHarmonicC1FromAmbientDiceSharpness(float sharpness)
{
    return exp2(3.37607 * pow(abs(sharpness * 1.45269 + 6.46623), -1.88874)) * 20.0 + -19.9337;
}


float ComputeZonalHarmonicC2FromAmbientDiceSharpness(float sharpness)
{
    float lhs = 0.239989 + 0.42846 * sharpness + -0.202951 * sharpness * sharpness + 0.0303908 * sharpness * sharpness * sharpness;
    float rhs = exp2(-1.44747 * pow(abs(sharpness * 0.644014 + -0.188877), -0.94422)) * -0.970862 + 0.967661;
    return sharpness > 2.33 ? rhs : lhs;
}
#endif

float3 ComputeZonalHarmonicFromAmbientDiceSharpness(float sharpness)
{
    return float3(
        ComputeZonalHarmonicC0FromAmbientDiceSharpness(sharpness),
        ComputeZonalHarmonicC1FromAmbientDiceSharpness(sharpness),
        ComputeZonalHarmonicC2FromAmbientDiceSharpness(sharpness)
    );
}

float3 ComputeZonalHarmonicFromAmbientDiceWrappedSharpness(float sharpness)
{
    return float3(
        ComputeZonalHarmonicC0FromAmbientDiceWrappedSharpness(sharpness),
        ComputeZonalHarmonicC1FromAmbientDiceWrappedSharpness(sharpness),
        ComputeZonalHarmonicC2FromAmbientDiceWrappedSharpness(sharpness)
    );
}

#endif
