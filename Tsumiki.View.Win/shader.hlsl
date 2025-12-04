cbuffer cbWorldTransform : register(b0)
{
    float4 Scale;
    float4 Location;
};

cbuffer cbGraph : register(b1)
{
    float GraphX;
    float GraphY;
    float GraphPitch;
    float GraphPeriod;
};

cbuffer cbFm : register(b2)
{
    float FmX;
    float FmY;
    float FmPitch;
    float FmPeriod;
    float FmLevel;
};

struct VS_INPUT
{
    float4 Pos : POSITION;
    float2 Tex : TEXCOORD;
};

struct PS_INPUT
{
    float4 Pos : SV_POSITION;
    float2 Tex : TEXCOORD;
};

Texture2D renderTexture : register(t0);
SamplerState samplerState : register(s0);

PS_INPUT VS(VS_INPUT input)
{
    PS_INPUT output;

    output.Pos = input.Pos * Scale + Location;
    output.Tex = input.Tex;

    return output;
}

float4 PixelImage(PS_INPUT input) : SV_TARGET
{
    float2 texDx = ddx(input.Tex) / 8.0;
    float2 texDy = ddy(input.Tex) / 8.0;
    float4 texColor = renderTexture.Sample(samplerState, input.Tex - texDx * 3 - texDy * 3)
                    + renderTexture.Sample(samplerState, input.Tex - texDx * 1 - texDy * 1)
                    + renderTexture.Sample(samplerState, input.Tex - texDx * 1 + texDy * 3)
                    + renderTexture.Sample(samplerState, input.Tex - texDx * 3 + texDy * 1)
                    + renderTexture.Sample(samplerState, input.Tex + texDx * 1 - texDy * 3)
                    + renderTexture.Sample(samplerState, input.Tex + texDx * 3 - texDy * 1)
                    + renderTexture.Sample(samplerState, input.Tex + texDx * 1 + texDy * 1)
                    + renderTexture.Sample(samplerState, input.Tex + texDx * 3 + texDy * 3);
    texColor /= 8;
    return texColor;
}

float4 PixelFilter(PS_INPUT input) : SV_TARGET
{
    float x = input.Tex.x;
    float y = input.Tex.y;
    float dy = abs(ddy(y));

    float t = exp2(20 * (x - GraphX));
    float vt = 1.0 - t;
    float fx = 0.25 - log2(vt * vt + 2.0 * t * GraphY) / 4.0;
    float dfdx = abs(ddx(fx));
    
    float dist = y - fx;
    float normalizedDist = dist / (dfdx + dy);
    float alpha = clamp(-0.5, 0.5, -normalizedDist) + 0.5;
    
    return float4(1.0, 1.0, 1.0, alpha);
}

float4 PixelModulator(PS_INPUT input) : SV_TARGET
{
    float x = input.Tex.x;
    float y = input.Tex.y;
    float dy = abs(ddy(y));

    float overdrive = min(1, 1 - GraphY);
    float t = (x - GraphPeriod * trunc(x / GraphPeriod)) * GraphPitch;
    float upY = min(1, abs(2 * (t - round(t)) / (GraphX * overdrive)));
    float downY = -2 * (t - 0.5 - floor(t)) / ((1 - GraphX) * overdrive);
    float angular = clamp(downY, -upY, upY);
    float fx = (sin(1.57079632 * angular) * min(1, GraphY + 1) + angular * max(0, -GraphY)) * 0.5 + 0.5;
    float dfdx = abs(ddx(fx));
    
    float dist = (y - fx) * sign(y - 0.5);
    float normalizedDist = dist / (dfdx + dy);
    float alpha = clamp(-0.5, 0.5, -normalizedDist) + 0.5;
    
    return float4(1.0, 1.0, 1.0, alpha);
}

float4 PixelCarrier(PS_INPUT input) : SV_TARGET
{
    float x = input.Tex.x;
    float y = input.Tex.y;
    float dy = abs(ddy(y));

    float overdrive = min(1, 1 - FmY);
    float t = (x - FmPeriod * trunc(x / FmPeriod)) * FmPitch;
    float upY = min(1, abs(2 * (t - round(t)) / (FmX * overdrive)));
    float downY = -2 * (t - 0.5 - floor(t)) / ((1 - FmX) * overdrive);
    float angular = clamp(downY, -upY, upY);
    float fm = sin(1.57079632 * angular) * min(1, FmY + 1) + angular * max(0, -FmY);
    
    overdrive = min(1, 1 - GraphY);
    t = (x - GraphPeriod * trunc(x / GraphPeriod)) * GraphPitch + fm * FmLevel;
    upY = min(1, abs(2 * (t - round(t)) / (GraphX * overdrive)));
    downY = -2 * (t - 0.5 - floor(t)) / ((1 - GraphX) * overdrive);
    angular = clamp(downY, -upY, upY);
    float fx = (sin(1.57079632 * angular) * min(1, GraphY + 1) + angular * max(0, -GraphY)) * 0.5 + 0.5;
    float dfdx = abs(ddx(fx));
    
    float dist = (y - fx) * sign(y - 0.5);
    float normalizedDist = dist / (dfdx + dy);
    float alpha = clamp(-0.5, 0.5, -normalizedDist) + 0.5;
    
    return float4(1.0, 1.0, 1.0, alpha);
}
