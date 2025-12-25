using System;

namespace Tsumiki.View.Mac;

/// <summary>Metal レンダラーのラッパー</summary>
internal sealed class Renderer : IDisposable, IDrawingContext
{
    private readonly nint _rendererHandle;
    private TabPageType _currentTabPageType;
    private bool _disposed;

    public Renderer(nint nsView, int width, int height)
    {
        // シェーダーライブラリデータを取得
        var shaderData = ShaderLibrary.MetalLibraryData;

        unsafe
        {
            fixed (byte* ptr = shaderData)
            {
                _rendererHandle = MacInterop.tsumiki_renderer_create(
                    nsView,
                    width,
                    height,
                    (nint)ptr,
                    shaderData.Length);
            }
        }

        if (_rendererHandle == nint.Zero)
        {
            throw new InvalidOperationException("Failed to create Metal renderer");
        }

        // リソース画像をロード
        LoadResourceImages();

        // 初回クリア
        Clear();
        MacInterop.tsumiki_renderer_present(_rendererHandle);
    }

    private void LoadResourceImages()
    {
        // Main、Mod、Tuning の順にロード
        Resources.ImageResource.LoadMain(_rendererHandle);
        Resources.ImageResource.LoadMod(_rendererHandle);
        Resources.ImageResource.LoadTuning(_rendererHandle);

        // デフォルトで Main を設定
        SetResourceImage(TabPageType.Main);
    }

    public void Resize(int width, int height)
    {
        ThrowIfDisposed();
        MacInterop.tsumiki_renderer_resize(_rendererHandle, width, height);
    }

    // IDrawingContext 実装

    public void Clear()
    {
        ThrowIfDisposed();
        MacInterop.tsumiki_renderer_clear(_rendererHandle);
    }

    public void SetResourceImage(TabPageType tabPageType)
    {
        ThrowIfDisposed();

        if (tabPageType == _currentTabPageType)
        {
            return;
        }

        _currentTabPageType = tabPageType;
        MacInterop.tsumiki_renderer_set_resource_image(_rendererHandle, (int)tabPageType);
    }

    public void DrawImage(in RectF clientRange, in RectF imageRange)
    {
        ThrowIfDisposed();
        MacInterop.tsumiki_renderer_draw_image(_rendererHandle, clientRange, imageRange);
    }

    public void DrawFilterGraph(in RectF clientRange, float normalizedCutoff, float resonance)
    {
        ThrowIfDisposed();
        MacInterop.tsumiki_renderer_draw_filter_graph(_rendererHandle, clientRange, normalizedCutoff, resonance);
    }

    public void DrawCarrierGraph(in RectF clientRange, in GraphParameters parameters, in FmParameters fmParameters)
    {
        ThrowIfDisposed();
        MacInterop.tsumiki_renderer_draw_carrier_graph(_rendererHandle, clientRange, parameters, fmParameters);
    }

    public void DrawModulatorGraph(in RectF clientRange, in GraphParameters parameters)
    {
        ThrowIfDisposed();
        MacInterop.tsumiki_renderer_draw_modulator_graph(_rendererHandle, clientRange, parameters);
    }

    public void Present()
    {
        ThrowIfDisposed();
        MacInterop.tsumiki_renderer_present(_rendererHandle);
    }

    // IDisposable 実装

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_rendererHandle != nint.Zero)
        {
            MacInterop.tsumiki_renderer_destroy(_rendererHandle);
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    ~Renderer()
    {
        Dispose();
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(Renderer));
        }
    }
}
