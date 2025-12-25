//
// Tsumiki Metal レンダラー C API
//

#ifndef TSUMIKI_METAL_H
#define TSUMIKI_METAL_H

#include <stdint.h>

#ifdef __cplusplus
extern "C" {
#endif

// 不透明ポインタ型
typedef void* TsumikiRendererHandle;

// 構造体定義（C# 側と完全一致させる必要がある）
typedef struct {
    float x, y, z, w;  // 位置
    float u, v;        // テクスチャ座標
} TsumikiVertex;

typedef struct {
    float x;
    float y;
    float pitch;
    float period;
} TsumikiGraphParameters;

typedef struct {
    float x;
    float y;
    float pitch;
    float period;
    float level;
    float _padding[3];  // アライメント用
} TsumikiFmParameters;

typedef struct {
    float left;
    float top;
    float right;
    float bottom;
} TsumikiRectF;

typedef enum {
    TsumikiTabPageMain = 0,
    TsumikiTabPageModulation = 1,
    TsumikiTabPageTuning = 2
} TsumikiTabPageType;

// レンダラーの作成・破棄
TsumikiRendererHandle tsumiki_renderer_create(void* nsView, int width, int height, const uint8_t* shaderData, int shaderDataLength);
void tsumiki_renderer_destroy(TsumikiRendererHandle handle);

// リサイズ
void tsumiki_renderer_resize(TsumikiRendererHandle handle, int width, int height);

// テクスチャ管理
void tsumiki_renderer_load_texture(TsumikiRendererHandle handle, const uint8_t* data, int width, int height);
void tsumiki_renderer_set_resource_image(TsumikiRendererHandle handle, TsumikiTabPageType type);

// 描画コマンド
void tsumiki_renderer_clear(TsumikiRendererHandle handle);
void tsumiki_renderer_draw_image(TsumikiRendererHandle handle, const TsumikiRectF* clientRange, const TsumikiRectF* imageRange);
void tsumiki_renderer_draw_filter_graph(TsumikiRendererHandle handle, const TsumikiRectF* clientRange, float normalizedCutoff, float resonance);
void tsumiki_renderer_draw_modulator_graph(TsumikiRendererHandle handle, const TsumikiRectF* clientRange, const TsumikiGraphParameters* parameters);
void tsumiki_renderer_draw_carrier_graph(TsumikiRendererHandle handle, const TsumikiRectF* clientRange, const TsumikiGraphParameters* graphParams, const TsumikiFmParameters* fmParams);

// フレーム提示
void tsumiki_renderer_present(TsumikiRendererHandle handle);

#ifdef __cplusplus
}
#endif

#endif // TSUMIKI_METAL_H
