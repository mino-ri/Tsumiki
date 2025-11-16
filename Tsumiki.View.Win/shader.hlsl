cbuffer cbWorldTransform : register(b0)
{
    matrix WorldViewProj;
};

struct VS_INPUT
{
    float4 Pos : POSITION;
    float4 Col : COLOR;
    float2 Tex : TEXCOORD;
};

struct PS_INPUT
{
    float4 Pos : SV_POSITION;
    float4 Col : COLOR;
    float2 Tex : TEXCOORD;
};

Texture2D renderTexture : register(t0);
SamplerState samplerState : register(s0);

PS_INPUT VS(VS_INPUT input)
{
    PS_INPUT output;

    output.Pos = mul(input.Pos, WorldViewProj);
    output.Col = input.Col;
    output.Tex = input.Tex;

    return output;
}

float4 PS(PS_INPUT input) : SV_TARGET
{
    float2 texDx = ddx(input.Tex) / 3.0;
    float2 texDy = ddy(input.Tex) / 3.0;
    float4 texColor;
    for (int x = -1; x <= 1; x++)
    {
        for (int y = -1; y <= 1; y++)
        {
            texColor += renderTexture.Sample(samplerState, input.Tex + texDx * x + texDy * y);
        }
    }
    texColor /= 9;
    return input.Col * texColor;
}
