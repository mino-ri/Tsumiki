using System;

namespace Tsumiki.View.Mac;

/// <summary>Mac 版 Tsumiki キャンバス実装</summary>
public sealed class TsumikiCanvas : ITsumikiCanvas
{
    private readonly nint _nsView;
    private readonly Renderer _renderer;
    private readonly IControl _control;
    private int _width;
    private int _height;
    private bool _isDisposed;

    /// <summary>TsumikiCanvas を作成します</summary>
    /// <param name="parentHandle">親 NSView のハンドル</param>
    /// <param name="rect">描画領域</param>
    /// <param name="control">コントロール</param>
    /// <returns>作成された TsumikiCanvas、または失敗時は null</returns>
    public static ITsumikiCanvas? Create(nint parentHandle, Rect rect, IControl control)
    {
        if (parentHandle == nint.Zero)
        {
            return null;
        }

        try
        {
            return new TsumikiCanvas(parentHandle, rect.Width, rect.Height, control);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating TsumikiCanvas: {ex}");
            return null;
        }
    }

    private TsumikiCanvas(nint nsView, int width, int height, IControl control)
    {
        _nsView = nsView;
        _width = width;
        _height = height;
        _control = control;

        // レンダラーを作成
        _renderer = new Renderer(nsView, width, height);

        // レンダリング要求イベントをサブスクライブ
        _control.RenderRequested += OnRenderRequested;
    }

    private void OnRenderRequested(IVisual visual)
    {
        // ビジュアルをレンダリング
        visual.Render(_renderer);

        // フレームを提示
        _renderer.Present();
    }

    public void Resize(Rect rect)
    {
        // アスペクト比 3:2 を維持（Windows 版と同じロジック）
        var normalizedByWidth = new Rect(rect.Left, rect.Top,
            rect.Left + rect.Width / 3 * 3, rect.Top + rect.Width / 3 * 2);
        var normalizedByHeight = new Rect(rect.Left, rect.Top,
            rect.Left + rect.Height / 2 * 3, rect.Top + rect.Height / 2 * 2);
        var normalized = normalizedByWidth.Width < normalizedByHeight.Width
            ? normalizedByWidth
            : normalizedByHeight;

        _width = normalized.Width;
        _height = normalized.Height;

        // レンダラーをリサイズ
        _renderer.Resize(normalized.Width, normalized.Height);

        // 再描画が必要な場合はここでトリガー
        // （通常は RenderRequested イベントで処理される）
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        // イベントハンドラを解除
        _control.RenderRequested -= OnRenderRequested;

        // レンダラーを破棄
        _renderer?.Dispose();

        _isDisposed = true;
        GC.SuppressFinalize(this);
    }

    ~TsumikiCanvas()
    {
        Dispose();
    }
}
