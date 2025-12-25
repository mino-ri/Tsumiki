//
// Tsumiki Metal レンダラー実装
//

#import <Metal/Metal.h>
#import <MetalKit/MetalKit.h>
#import <QuartzCore/QuartzCore.h>
#import <Cocoa/Cocoa.h>
#import "TsumikiMetal.h"

// WorldTransform 定数バッファ
typedef struct {
    simd_float4 scale;
    simd_float4 location;
} WorldTransform;

// MetalRenderer クラス
@interface MetalRenderer : NSObject

@property (nonatomic, strong) id<MTLDevice> device;
@property (nonatomic, strong) id<MTLCommandQueue> commandQueue;
@property (nonatomic, strong) CAMetalLayer* metalLayer;

// パイプライン
@property (nonatomic, strong) id<MTLRenderPipelineState> imagePipeline;
@property (nonatomic, strong) id<MTLRenderPipelineState> filterPipeline;
@property (nonatomic, strong) id<MTLRenderPipelineState> modulatorPipeline;
@property (nonatomic, strong) id<MTLRenderPipelineState> carrierPipeline;

// バッファ
@property (nonatomic, strong) id<MTLBuffer> vertexBuffer;
@property (nonatomic, strong) id<MTLBuffer> worldTransformBuffer;
@property (nonatomic, strong) id<MTLBuffer> graphParametersBuffer;
@property (nonatomic, strong) id<MTLBuffer> fmParametersBuffer;

// テクスチャ
@property (nonatomic, strong) id<MTLTexture> mainTexture;
@property (nonatomic, strong) id<MTLTexture> modTexture;
@property (nonatomic, strong) id<MTLTexture> tuningTexture;
@property (nonatomic, strong) id<MTLTexture> currentTexture;
@property (nonatomic, strong) id<MTLSamplerState> samplerState;

// 現在のコマンドバッファ（差分描画用）
@property (nonatomic, strong) id<MTLCommandBuffer> currentCommandBuffer;
@property (nonatomic, strong) id<MTLRenderCommandEncoder> currentEncoder;
@property (nonatomic, strong) id<CAMetalDrawable> currentDrawable;

@property (nonatomic, assign) int width;
@property (nonatomic, assign) int height;

- (instancetype)initWithView:(NSView*)view width:(int)width height:(int)height shaderData:(NSData*)shaderData;
- (void)resize:(int)width height:(int)height;
- (void)loadTexture:(const uint8_t*)data width:(int)width height:(int)height;
- (void)setResourceImage:(TsumikiTabPageType)type;
- (void)clear;
- (void)drawImage:(const TsumikiRectF*)clientRange imageRange:(const TsumikiRectF*)imageRange;
- (void)drawFilterGraph:(const TsumikiRectF*)clientRange cutoff:(float)cutoff resonance:(float)resonance;
- (void)drawModulatorGraph:(const TsumikiRectF*)clientRange parameters:(const TsumikiGraphParameters*)parameters;
- (void)drawCarrierGraph:(const TsumikiRectF*)clientRange graphParams:(const TsumikiGraphParameters*)graphParams fmParams:(const TsumikiFmParameters*)fmParams;
- (void)present;

@end

@implementation MetalRenderer

- (instancetype)initWithView:(NSView*)view width:(int)width height:(int)height shaderData:(NSData*)shaderData
{
    self = [super init];
    if (self) {
        _width = width;
        _height = height;

        // Metal デバイスとコマンドキューの作成
        _device = MTLCreateSystemDefaultDevice();
        if (!_device) {
            NSLog(@"Error: Metal is not supported on this device");
            return nil;
        }

        _commandQueue = [_device newCommandQueue];

        // CAMetalLayer の設定
        _metalLayer = [CAMetalLayer layer];
        _metalLayer.device = _device;
        _metalLayer.pixelFormat = MTLPixelFormatBGRA8Unorm;
        _metalLayer.framebufferOnly = NO;  // テクスチャとして読み取り可能にする（必要に応じて）
        _metalLayer.drawableSize = CGSizeMake(width, height);

        // VSync 有効化
        _metalLayer.displaySyncEnabled = YES;

        // NSView に追加
        view.wantsLayer = YES;
        view.layer = _metalLayer;

        // 背景色を設定（0x504530）
        view.layer.backgroundColor = CGColorCreateGenericRGB(0x50/255.0, 0x45/255.0, 0x30/255.0, 1.0);

        // シェーダーライブラリのロード
        NSError* error = nil;
        id<MTLLibrary> library = [_device newLibraryWithData:shaderData error:&error];
        if (!library) {
            NSLog(@"Error loading shader library: %@", error);
            return nil;
        }

        // パイプラインの作成
        if (![self setupPipelinesWithLibrary:library]) {
            return nil;
        }

        // バッファの作成
        [self setupBuffers];

        // サンプラーの作成
        [self setupSampler];
    }
    return self;
}

- (BOOL)setupPipelinesWithLibrary:(id<MTLLibrary>)library
{
    MTLRenderPipelineDescriptor* descriptor = [[MTLRenderPipelineDescriptor alloc] init];

    // 頂点関数（共通）
    id<MTLFunction> vertexFunction = [library newFunctionWithName:@"vertexShader"];
    descriptor.vertexFunction = vertexFunction;

    // カラーアタッチメント設定
    descriptor.colorAttachments[0].pixelFormat = MTLPixelFormatBGRA8Unorm;

    // アルファブレンディング設定
    descriptor.colorAttachments[0].blendingEnabled = YES;
    descriptor.colorAttachments[0].sourceRGBBlendFactor = MTLBlendFactorSourceAlpha;
    descriptor.colorAttachments[0].destinationRGBBlendFactor = MTLBlendFactorOneMinusSourceAlpha;
    descriptor.colorAttachments[0].sourceAlphaBlendFactor = MTLBlendFactorOne;
    descriptor.colorAttachments[0].destinationAlphaBlendFactor = MTLBlendFactorZero;

    // 頂点記述子（POSITION: float4, TEXCOORD: float2）
    MTLVertexDescriptor* vertexDescriptor = [[MTLVertexDescriptor alloc] init];
    vertexDescriptor.attributes[0].format = MTLVertexFormatFloat4;
    vertexDescriptor.attributes[0].offset = 0;
    vertexDescriptor.attributes[0].bufferIndex = 0;
    vertexDescriptor.attributes[1].format = MTLVertexFormatFloat2;
    vertexDescriptor.attributes[1].offset = 16;
    vertexDescriptor.attributes[1].bufferIndex = 0;
    vertexDescriptor.layouts[0].stride = sizeof(TsumikiVertex);
    descriptor.vertexDescriptor = vertexDescriptor;

    NSError* error = nil;

    // 画像パイプライン
    descriptor.fragmentFunction = [library newFunctionWithName:@"imagePixelShader"];
    _imagePipeline = [_device newRenderPipelineStateWithDescriptor:descriptor error:&error];
    if (!_imagePipeline) {
        NSLog(@"Error creating image pipeline: %@", error);
        return NO;
    }

    // フィルターパイプライン
    descriptor.fragmentFunction = [library newFunctionWithName:@"filterPixelShader"];
    _filterPipeline = [_device newRenderPipelineStateWithDescriptor:descriptor error:&error];
    if (!_filterPipeline) {
        NSLog(@"Error creating filter pipeline: %@", error);
        return NO;
    }

    // モジュレーターパイプライン
    descriptor.fragmentFunction = [library newFunctionWithName:@"modulatorPixelShader"];
    _modulatorPipeline = [_device newRenderPipelineStateWithDescriptor:descriptor error:&error];
    if (!_modulatorPipeline) {
        NSLog(@"Error creating modulator pipeline: %@", error);
        return NO;
    }

    // キャリアパイプライン
    descriptor.fragmentFunction = [library newFunctionWithName:@"carrierPixelShader"];
    _carrierPipeline = [_device newRenderPipelineStateWithDescriptor:descriptor error:&error];
    if (!_carrierPipeline) {
        NSLog(@"Error creating carrier pipeline: %@", error);
        return NO;
    }

    return YES;
}

- (void)setupBuffers
{
    // 頂点バッファ（4頂点、TriangleStrip）
    _vertexBuffer = [_device newBufferWithLength:sizeof(TsumikiVertex) * 4
                                         options:MTLResourceStorageModeShared];

    // WorldTransform 定数バッファ
    _worldTransformBuffer = [_device newBufferWithLength:sizeof(WorldTransform)
                                                 options:MTLResourceStorageModeShared];

    // WorldTransform を初期化（スケール: 2, -2, 位置: -1, 1）
    WorldTransform* transform = (WorldTransform*)[_worldTransformBuffer contents];
    transform->scale = simd_make_float4(2.0f, -2.0f, 1.0f, 1.0f);  // Y軸反転
    transform->location = simd_make_float4(-1.0f, 1.0f, 0.0f, 0.0f);

    // GraphParameters 定数バッファ
    _graphParametersBuffer = [_device newBufferWithLength:sizeof(TsumikiGraphParameters)
                                                  options:MTLResourceStorageModeShared];

    // FmParameters 定数バッファ
    _fmParametersBuffer = [_device newBufferWithLength:sizeof(TsumikiFmParameters)
                                               options:MTLResourceStorageModeShared];
}

- (void)setupSampler
{
    MTLSamplerDescriptor* samplerDescriptor = [[MTLSamplerDescriptor alloc] init];
    samplerDescriptor.minFilter = MTLSamplerMinMagFilterLinear;
    samplerDescriptor.magFilter = MTLSamplerMinMagFilterLinear;
    samplerDescriptor.sAddressMode = MTLSamplerAddressModeClampToEdge;  // Border モードの代替
    samplerDescriptor.tAddressMode = MTLSamplerAddressModeClampToEdge;
    _samplerState = [_device newSamplerStateWithDescriptor:samplerDescriptor];
}

- (void)resize:(int)width height:(int)height
{
    _width = width;
    _height = height;
    _metalLayer.drawableSize = CGSizeMake(width, height);
}

- (void)loadTexture:(const uint8_t*)data width:(int)width height:(int)height
{
    MTLTextureDescriptor* descriptor = [MTLTextureDescriptor texture2DDescriptorWithPixelFormat:MTLPixelFormatBGRA8Unorm
                                                                                          width:width
                                                                                         height:height
                                                                                      mipmapped:NO];

    id<MTLTexture> texture = [_device newTextureWithDescriptor:descriptor];

    MTLRegion region = MTLRegionMake2D(0, 0, width, height);
    [texture replaceRegion:region mipmapLevel:0 withBytes:data bytesPerRow:width * 4];

    // テクスチャを保存（初回ロード時に判定）
    if (!_mainTexture) {
        _mainTexture = texture;
        _currentTexture = texture;
    } else if (!_modTexture) {
        _modTexture = texture;
    } else if (!_tuningTexture) {
        _tuningTexture = texture;
    }
}

- (void)setResourceImage:(TsumikiTabPageType)type
{
    switch (type) {
        case TsumikiTabPageMain:
            _currentTexture = _mainTexture;
            break;
        case TsumikiTabPageModulation:
            _currentTexture = _modTexture;
            break;
        case TsumikiTabPageTuning:
            _currentTexture = _tuningTexture;
            break;
    }
}

- (void)beginRenderPass
{
    if (_currentEncoder) {
        return;  // すでに開始済み
    }

    _currentDrawable = [_metalLayer nextDrawable];
    if (!_currentDrawable) {
        NSLog(@"Error: Failed to get drawable");
        return;
    }

    _currentCommandBuffer = [_commandQueue commandBuffer];

    MTLRenderPassDescriptor* passDescriptor = [MTLRenderPassDescriptor renderPassDescriptor];
    passDescriptor.colorAttachments[0].texture = _currentDrawable.texture;
    passDescriptor.colorAttachments[0].loadAction = MTLLoadActionLoad;  // 差分描画
    passDescriptor.colorAttachments[0].storeAction = MTLStoreActionStore;

    _currentEncoder = [_currentCommandBuffer renderCommandEncoderWithDescriptor:passDescriptor];
}

- (void)clear
{
    // clear は最初のレンダーパスで MTLLoadActionClear を使う
    if (_currentEncoder) {
        [_currentEncoder endEncoding];
        _currentEncoder = nil;
    }

    _currentDrawable = [_metalLayer nextDrawable];
    if (!_currentDrawable) {
        return;
    }

    _currentCommandBuffer = [_commandQueue commandBuffer];

    MTLRenderPassDescriptor* passDescriptor = [MTLRenderPassDescriptor renderPassDescriptor];
    passDescriptor.colorAttachments[0].texture = _currentDrawable.texture;
    passDescriptor.colorAttachments[0].loadAction = MTLLoadActionClear;
    passDescriptor.colorAttachments[0].clearColor = MTLClearColorMake(0x50/255.0, 0x45/255.0, 0x30/255.0, 1.0);
    passDescriptor.colorAttachments[0].storeAction = MTLStoreActionStore;

    _currentEncoder = [_currentCommandBuffer renderCommandEncoderWithDescriptor:passDescriptor];
    [_currentEncoder endEncoding];
    _currentEncoder = nil;
}

- (void)drawImage:(const TsumikiRectF*)clientRange imageRange:(const TsumikiRectF*)imageRange
{
    [self beginRenderPass];
    if (!_currentEncoder) return;

    // 頂点バッファを更新
    TsumikiVertex* vertices = (TsumikiVertex*)[_vertexBuffer contents];
    vertices[0] = (TsumikiVertex){clientRange->left, clientRange->top, 0.5f, 1.0f, imageRange->left, imageRange->top};
    vertices[1] = (TsumikiVertex){clientRange->right, clientRange->top, 0.5f, 1.0f, imageRange->right, imageRange->top};
    vertices[2] = (TsumikiVertex){clientRange->left, clientRange->bottom, 0.5f, 1.0f, imageRange->left, imageRange->bottom};
    vertices[3] = (TsumikiVertex){clientRange->right, clientRange->bottom, 0.5f, 1.0f, imageRange->right, imageRange->bottom};

    // パイプラインとバッファを設定
    [_currentEncoder setRenderPipelineState:_imagePipeline];
    [_currentEncoder setVertexBuffer:_vertexBuffer offset:0 atIndex:0];
    [_currentEncoder setVertexBuffer:_worldTransformBuffer offset:0 atIndex:1];
    [_currentEncoder setFragmentTexture:_currentTexture atIndex:0];
    [_currentEncoder setFragmentSamplerState:_samplerState atIndex:0];

    // 描画
    [_currentEncoder drawPrimitives:MTLPrimitiveTypeTriangleStrip vertexStart:0 vertexCount:4];
}

- (void)drawFilterGraph:(const TsumikiRectF*)clientRange cutoff:(float)cutoff resonance:(float)resonance
{
    [self beginRenderPass];
    if (!_currentEncoder) return;

    // 頂点バッファを更新
    TsumikiVertex* vertices = (TsumikiVertex*)[_vertexBuffer contents];
    vertices[0] = (TsumikiVertex){clientRange->left, clientRange->top, 0.5f, 1.0f, 0.0f, 0.0f};
    vertices[1] = (TsumikiVertex){clientRange->right, clientRange->top, 0.5f, 1.0f, 1.0f, 0.0f};
    vertices[2] = (TsumikiVertex){clientRange->left, clientRange->bottom, 0.5f, 1.0f, 0.0f, 1.0f};
    vertices[3] = (TsumikiVertex){clientRange->right, clientRange->bottom, 0.5f, 1.0f, 1.0f, 1.0f};

    // GraphParameters を設定
    TsumikiGraphParameters* params = (TsumikiGraphParameters*)[_graphParametersBuffer contents];
    params->x = cutoff;
    params->y = resonance;
    params->pitch = 0.0f;
    params->period = 0.0f;

    // パイプラインとバッファを設定
    [_currentEncoder setRenderPipelineState:_filterPipeline];
    [_currentEncoder setVertexBuffer:_vertexBuffer offset:0 atIndex:0];
    [_currentEncoder setVertexBuffer:_worldTransformBuffer offset:0 atIndex:1];
    [_currentEncoder setFragmentBuffer:_graphParametersBuffer offset:0 atIndex:1];

    // 描画
    [_currentEncoder drawPrimitives:MTLPrimitiveTypeTriangleStrip vertexStart:0 vertexCount:4];
}

- (void)drawModulatorGraph:(const TsumikiRectF*)clientRange parameters:(const TsumikiGraphParameters*)parameters
{
    [self beginRenderPass];
    if (!_currentEncoder) return;

    // 頂点バッファを更新
    TsumikiVertex* vertices = (TsumikiVertex*)[_vertexBuffer contents];
    vertices[0] = (TsumikiVertex){clientRange->left, clientRange->top, 0.5f, 1.0f, 0.0f, 0.0f};
    vertices[1] = (TsumikiVertex){clientRange->right, clientRange->top, 0.5f, 1.0f, 1.0f, 0.0f};
    vertices[2] = (TsumikiVertex){clientRange->left, clientRange->bottom, 0.5f, 1.0f, 0.0f, 1.0f};
    vertices[3] = (TsumikiVertex){clientRange->right, clientRange->bottom, 0.5f, 1.0f, 1.0f, 1.0f};

    // GraphParameters をコピー
    memcpy([_graphParametersBuffer contents], parameters, sizeof(TsumikiGraphParameters));

    // パイプラインとバッファを設定
    [_currentEncoder setRenderPipelineState:_modulatorPipeline];
    [_currentEncoder setVertexBuffer:_vertexBuffer offset:0 atIndex:0];
    [_currentEncoder setVertexBuffer:_worldTransformBuffer offset:0 atIndex:1];
    [_currentEncoder setFragmentBuffer:_graphParametersBuffer offset:0 atIndex:1];

    // 描画
    [_currentEncoder drawPrimitives:MTLPrimitiveTypeTriangleStrip vertexStart:0 vertexCount:4];
}

- (void)drawCarrierGraph:(const TsumikiRectF*)clientRange graphParams:(const TsumikiGraphParameters*)graphParams fmParams:(const TsumikiFmParameters*)fmParams
{
    [self beginRenderPass];
    if (!_currentEncoder) return;

    // 頂点バッファを更新
    TsumikiVertex* vertices = (TsumikiVertex*)[_vertexBuffer contents];
    vertices[0] = (TsumikiVertex){clientRange->left, clientRange->top, 0.5f, 1.0f, 0.0f, 0.0f};
    vertices[1] = (TsumikiVertex){clientRange->right, clientRange->top, 0.5f, 1.0f, 1.0f, 0.0f};
    vertices[2] = (TsumikiVertex){clientRange->left, clientRange->bottom, 0.5f, 1.0f, 0.0f, 1.0f};
    vertices[3] = (TsumikiVertex){clientRange->right, clientRange->bottom, 0.5f, 1.0f, 1.0f, 1.0f};

    // GraphParameters と FmParameters をコピー
    memcpy([_graphParametersBuffer contents], graphParams, sizeof(TsumikiGraphParameters));
    memcpy([_fmParametersBuffer contents], fmParams, sizeof(TsumikiFmParameters));

    // パイプラインとバッファを設定
    [_currentEncoder setRenderPipelineState:_carrierPipeline];
    [_currentEncoder setVertexBuffer:_vertexBuffer offset:0 atIndex:0];
    [_currentEncoder setVertexBuffer:_worldTransformBuffer offset:0 atIndex:1];
    [_currentEncoder setFragmentBuffer:_graphParametersBuffer offset:0 atIndex:1];
    [_currentEncoder setFragmentBuffer:_fmParametersBuffer offset:0 atIndex:2];

    // 描画
    [_currentEncoder drawPrimitives:MTLPrimitiveTypeTriangleStrip vertexStart:0 vertexCount:4];
}

- (void)present
{
    if (_currentEncoder) {
        [_currentEncoder endEncoding];
        _currentEncoder = nil;
    }

    if (_currentCommandBuffer && _currentDrawable) {
        [_currentCommandBuffer presentDrawable:_currentDrawable];
        [_currentCommandBuffer commit];
        _currentCommandBuffer = nil;
        _currentDrawable = nil;
    }
}

@end

// C API 実装

TsumikiRendererHandle tsumiki_renderer_create(void* nsView, int width, int height, const uint8_t* shaderData, int shaderDataLength)
{
    NSView* view = (__bridge NSView*)nsView;
    NSData* data = [NSData dataWithBytes:shaderData length:shaderDataLength];

    MetalRenderer* renderer = [[MetalRenderer alloc] initWithView:view width:width height:height shaderData:data];
    return (__bridge_retained void*)renderer;
}

void tsumiki_renderer_destroy(TsumikiRendererHandle handle)
{
    if (handle) {
        MetalRenderer* renderer = (__bridge_transfer MetalRenderer*)handle;
        renderer = nil;
    }
}

void tsumiki_renderer_resize(TsumikiRendererHandle handle, int width, int height)
{
    MetalRenderer* renderer = (__bridge MetalRenderer*)handle;
    [renderer resize:width height:height];
}

void tsumiki_renderer_load_texture(TsumikiRendererHandle handle, const uint8_t* data, int width, int height)
{
    MetalRenderer* renderer = (__bridge MetalRenderer*)handle;
    [renderer loadTexture:data width:width height:height];
}

void tsumiki_renderer_set_resource_image(TsumikiRendererHandle handle, TsumikiTabPageType type)
{
    MetalRenderer* renderer = (__bridge MetalRenderer*)handle;
    [renderer setResourceImage:type];
}

void tsumiki_renderer_clear(TsumikiRendererHandle handle)
{
    MetalRenderer* renderer = (__bridge MetalRenderer*)handle;
    [renderer clear];
}

void tsumiki_renderer_draw_image(TsumikiRendererHandle handle, const TsumikiRectF* clientRange, const TsumikiRectF* imageRange)
{
    MetalRenderer* renderer = (__bridge MetalRenderer*)handle;
    [renderer drawImage:clientRange imageRange:imageRange];
}

void tsumiki_renderer_draw_filter_graph(TsumikiRendererHandle handle, const TsumikiRectF* clientRange, float normalizedCutoff, float resonance)
{
    MetalRenderer* renderer = (__bridge MetalRenderer*)handle;
    [renderer drawFilterGraph:clientRange cutoff:normalizedCutoff resonance:resonance];
}

void tsumiki_renderer_draw_modulator_graph(TsumikiRendererHandle handle, const TsumikiRectF* clientRange, const TsumikiGraphParameters* parameters)
{
    MetalRenderer* renderer = (__bridge MetalRenderer*)handle;
    [renderer drawModulatorGraph:clientRange parameters:parameters];
}

void tsumiki_renderer_draw_carrier_graph(TsumikiRendererHandle handle, const TsumikiRectF* clientRange, const TsumikiGraphParameters* graphParams, const TsumikiFmParameters* fmParams)
{
    MetalRenderer* renderer = (__bridge MetalRenderer*)handle;
    [renderer drawCarrierGraph:clientRange graphParams:graphParams fmParams:fmParams];
}

void tsumiki_renderer_present(TsumikiRendererHandle handle)
{
    MetalRenderer* renderer = (__bridge MetalRenderer*)handle;
    [renderer present];
}
