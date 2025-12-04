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

float ResonantFilter(float x)
{
    return 0.35 * sin(x * 9.0) + 0.5;
}

float4 PixelFilter(PS_INPUT input) : SV_TARGET
{
    float x = input.Tex.x;
    float y = input.Tex.y;
    // float dx = abs(ddx(x));
    float dy = abs(ddy(y));
    
    float fx = ResonantFilter(x);
    float dfdx = abs(ddx(fx));
    // より厳密なアンチエイリアスにするならこちら
    // float epsilon = 0.0009765625;
    // float dfdx = abs((ResonantFilter(x + epsilon) - ResonantFilter(x - epsilon))) / (2.0 * epsilon) * dx;
    
    // y方向の符号付き距離
    float dist = (y - fx) * sign(y - 0.5);
    
    // 正規化された距離とアンチエイリアス
    float normalizedDist = dist / (dfdx + dy);
    float alpha = clamp(-0.5, 0.5, -normalizedDist) + 0.5;
    
    return float4(alpha, alpha, alpha, 1.0);
}
