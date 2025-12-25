//
// Tsumiki Metal シェーダー
// HLSL (shader.hlsl) から移植
//

#include <metal_stdlib>
using namespace metal;

// 定数バッファ構造体
struct WorldTransform {
    float4 scale;
    float4 location;
};

struct GraphParameters {
    float x;
    float y;
    float pitch;
    float period;
};

struct FmParameters {
    float x;
    float y;
    float pitch;
    float period;
    float level;
    float3 _padding;
};

// 頂点入出力
struct VertexIn {
    float4 position [[attribute(0)]];
    float2 texCoord [[attribute(1)]];
};

struct VertexOut {
    float4 position [[position]];
    float2 texCoord;
};

// 頂点シェーダー
vertex VertexOut vertexShader(
    VertexIn in [[stage_in]],
    constant WorldTransform& transform [[buffer(1)]])
{
    VertexOut out;
    out.position = in.position * transform.scale + transform.location;
    out.texCoord = in.texCoord;
    return out;
}

// 画像ピクセルシェーダー（8x SSAA アンチエイリアス）
fragment float4 imagePixelShader(
    VertexOut in [[stage_in]],
    texture2d<float> renderTexture [[texture(0)]],
    sampler samplerState [[sampler(0)]])
{
    float2 texDx = dfdx(in.texCoord) / 8.0;
    float2 texDy = dfdy(in.texCoord) / 8.0;

    float4 texColor = renderTexture.sample(samplerState, in.texCoord - texDx * 3.0 - texDy * 3.0)
                    + renderTexture.sample(samplerState, in.texCoord - texDx * 1.0 - texDy * 1.0)
                    + renderTexture.sample(samplerState, in.texCoord - texDx * 1.0 + texDy * 3.0)
                    + renderTexture.sample(samplerState, in.texCoord - texDx * 3.0 + texDy * 1.0)
                    + renderTexture.sample(samplerState, in.texCoord + texDx * 1.0 - texDy * 3.0)
                    + renderTexture.sample(samplerState, in.texCoord + texDx * 3.0 - texDy * 1.0)
                    + renderTexture.sample(samplerState, in.texCoord + texDx * 1.0 + texDy * 1.0)
                    + renderTexture.sample(samplerState, in.texCoord + texDx * 3.0 + texDy * 3.0);

    return texColor / 8.0;
}

// フィルターグラフピクセルシェーダー
fragment float4 filterPixelShader(
    VertexOut in [[stage_in]],
    constant GraphParameters& graph [[buffer(1)]])
{
    float x = in.texCoord.x;
    float y = in.texCoord.y;
    float dy = abs(dfdy(y));

    float t = exp2(20.0 * (x - graph.x));
    float vt = 1.0 - t;
    float fx = 0.25 - log2(vt * vt + 2.0 * t * graph.y) / 4.0;
    float dfdx_fx = abs(dfdx(fx));

    float dist = y - fx;
    float normalizedDist = dist / (dfdx_fx + dy);
    float alpha = clamp(-normalizedDist, -0.5, 0.5) + 0.5;

    return float4(1.0, 1.0, 1.0, alpha);
}

// モジュレーターグラフピクセルシェーダー
fragment float4 modulatorPixelShader(
    VertexOut in [[stage_in]],
    constant GraphParameters& graph [[buffer(1)]])
{
    float x = in.texCoord.x;
    float y = in.texCoord.y;
    float dy = abs(dfdy(y));

    float overdrive = min(1.0, 1.0 - graph.y);
    float t = (x - graph.period * trunc(x / graph.period)) * graph.pitch;
    float upY = min(1.0, abs(2.0 * (t - round(t)) / (graph.x * overdrive)));
    float downY = -2.0 * (t - 0.5 - floor(t)) / ((1.0 - graph.x) * overdrive);
    float angular = clamp(downY, -upY, upY);
    float fx = (sin(1.57079632 * angular) * min(1.0, graph.y + 1.0) +
                angular * max(0.0, -graph.y)) * 0.5 + 0.5;
    float dfdx_fx = abs(dfdx(fx));

    float dist = (y - fx) * sign(y - 0.5);
    float normalizedDist = dist / (dfdx_fx + dy);
    float alpha = clamp(-normalizedDist, -0.5, 0.5) + 0.5;

    return float4(1.0, 1.0, 1.0, alpha);
}

// キャリアグラフピクセルシェーダー（FM 変調）
fragment float4 carrierPixelShader(
    VertexOut in [[stage_in]],
    constant GraphParameters& graph [[buffer(1)]],
    constant FmParameters& fm [[buffer(2)]])
{
    float x = in.texCoord.x;
    float y = in.texCoord.y;
    float dy = abs(dfdy(y));

    // FM 計算
    float overdrive = min(1.0, 1.0 - fm.y);
    float t = (x - fm.period * trunc(x / fm.period)) * fm.pitch;
    float upY = min(1.0, abs(2.0 * (t - round(t)) / (fm.x * overdrive)));
    float downY = -2.0 * (t - 0.5 - floor(t)) / ((1.0 - fm.x) * overdrive);
    float angular = clamp(downY, -upY, upY);
    float fmValue = sin(1.57079632 * angular) * min(1.0, fm.y + 1.0) +
                    angular * max(0.0, -fm.y);

    // キャリア計算（FM を加算）
    overdrive = min(1.0, 1.0 - graph.y);
    t = (x - graph.period * trunc(x / graph.period)) * graph.pitch + fmValue * fm.level;
    upY = min(1.0, abs(2.0 * (t - round(t)) / (graph.x * overdrive)));
    downY = -2.0 * (t - 0.5 - floor(t)) / ((1.0 - graph.x) * overdrive);
    angular = clamp(downY, -upY, upY);
    float fx = (sin(1.57079632 * angular) * min(1.0, graph.y + 1.0) +
                angular * max(0.0, -graph.y)) * 0.5 + 0.5;
    float dfdx_fx = abs(dfdx(fx));

    float dist = (y - fx) * sign(y - 0.5);
    float normalizedDist = dist / (dfdx_fx + dy);
    float alpha = clamp(-normalizedDist, -0.5, 0.5) + 0.5;

    return float4(1.0, 1.0, 1.0, alpha);
}
