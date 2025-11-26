cbuffer cbWorldTransform : register(b0)
{
    float4 Scale;
    float4 Location;
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

float4 PS(PS_INPUT input) : SV_TARGET
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
