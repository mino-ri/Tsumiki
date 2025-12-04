using System.Collections.Concurrent;
using System.Numerics;
using System.Runtime.InteropServices;
using IndirectX;
using IndirectX.D3D11;
using IndirectX.Dxgi;
using IndirectX.Helper;

namespace Tsumiki.View.Win;

internal sealed class Renderer : IDisposable, IDrawingContext
{
    [StructLayout(LayoutKind.Sequential)]
    private struct ConstantBuffer
    {
        public Vector4 Scale;
        public Vector4 Location;
    }

    private const int SyncInterval = 2;
    private readonly RectF _graphUvRect = new(0f, 1f, 1f, 0f);
    private readonly Color _background = new(0xFF504530);
    private readonly ConcurrentQueue<IVisual> _visuals = new();
    private readonly HashSet<IVisual> _renderedVisuals = [];
    private readonly Queue<IVisual> _prevVisuals = new();
    private readonly Graphics _graphics;
    private readonly IVisual _rootVisual;
    private readonly IResourceTexture _sourceTexture;
    private readonly ArrayBuffer<Vertex> _vertexBuffer;
    private readonly ValueBuffer<GraphParameters> _graphBuffer;
    private readonly ValueBuffer<FmParameters> _fmBuffer;
    private readonly PixelShader _imagePixelShader;
    private readonly PixelShader _filterPixelShader;
    private readonly PixelShader _modulatorPixelShader;
    private readonly PixelShader _carrierPixelShader;
    private (int width, int height)? _changedSize;

    public Renderer(nint hwnd, int width, int height, IVisual rootVisual)
    {
        _graphics = new Graphics(hwnd, width, height,
            windowed: true,
            refreshRate: 60,
            syncInterval: SyncInterval,
            useZBuffer: false);

        _imagePixelShader = ShaderSource.LoadImagePixelShader(_graphics.Device);
        _filterPixelShader = ShaderSource.LoadFilterPixelShader(_graphics.Device);
        _modulatorPixelShader = ShaderSource.LoadModulatorPixelShader(_graphics.Device);
        _carrierPixelShader = ShaderSource.LoadCarrierPixelShader(_graphics.Device);
        _graphics.SetVertexShader(ShaderSource.LoadVertexShader);
        _graphics.PixelShader = _imagePixelShader;
        _graphics.SetInputLayout(ShaderSource.LoadInputLayout,
            new InputElementDesc { SemanticName = "POSITION", Format = Format.R32G32B32A32Float },
            new InputElementDesc { SemanticName = "TEXCOORD", Format = Format.R32G32Float, AlignedByteOffset = 16 });

        _vertexBuffer = _graphics.RegisterVertexBuffer<Vertex>(0, 4);
        _vertexBuffer.Buffer[0] = new Vertex(0f, 0f, 0.5f, 1f, new Vector2(0f, 0f));
        _vertexBuffer.Buffer[1] = new Vertex(1f, 0f, 0.5f, 1f, new Vector2(0.75f, 0f));
        _vertexBuffer.Buffer[2] = new Vertex(0f, 1f, 0.5f, 1f, new Vector2(0f, 1f));
        _vertexBuffer.Buffer[3] = new Vertex(1f, 1f, 0.5f, 1f, new Vector2(0.75f, 1f));
        _graphics.RegisterConstantBuffer<ConstantBuffer>(0, ShaderStages.VertexShader).WriteByRef(
            new ConstantBuffer
            {
                Scale = new Vector4(2f, -2f, 1f, 1f),
                Location = new Vector4(-1f, 1f, 0f, 0f),
            });
        _graphBuffer = _graphics.RegisterConstantBuffer<GraphParameters>(1, ShaderStages.PixelShader);
        _fmBuffer = _graphics.RegisterConstantBuffer<FmParameters>(2, ShaderStages.PixelShader);

        _sourceTexture = Resources.ImageResource.LoadMain(_graphics);
        _graphics.SetTexture(0, _sourceTexture);
        _graphics.SetBorderSampler(0);
        using var alphaBlendState = _graphics.CreateAlphaBlendState();
        _graphics.SetBlendState(alphaBlendState);
        _graphics.Context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
        _rootVisual = rootVisual;
        _visuals.Enqueue(_rootVisual);
        Clear();
    }

    public void Frame()
    {
        try
        {
            if (_changedSize.HasValue)
            {
                var (width, height) = _changedSize.Value;
                _changedSize = null;
                _graphics.Resize(width, height);
                _visuals.Clear();
                _visuals.Enqueue(_rootVisual);
                Clear();
            }

            while (_prevVisuals.TryDequeue(out var visual))
            {
                visual.Render(this);
            }

            _renderedVisuals.Clear();
            while (_visuals.TryDequeue(out var visual))
            {
                if (_renderedVisuals.Add(visual))
                {
                    visual.Render(this);
                    _prevVisuals.Enqueue(visual);
                }
            }

            _graphics.SwapChain.TryPresent(SyncInterval, PresentFlags.None);
        }
        catch (Exception ex)
        {
            TsumikiLogger.WriteException(ex);
        }
    }

    public void Resize(int width, int height)
    {
        _changedSize = (width, height);
    }

    internal void RegisterVisual(IVisual visual)
    {
        _visuals.Enqueue(visual);
    }

    public void Clear()
    {
        _graphics.ClearRenderTarget(_background);
    }

    public void DrawImage(in RectF clientRange, in RectF imageRange)
    {
        _vertexBuffer.Buffer[0].Vector.X = clientRange.Left;
        _vertexBuffer.Buffer[0].Vector.Y = clientRange.Top;
        _vertexBuffer.Buffer[0].Texture.X = imageRange.Left;
        _vertexBuffer.Buffer[0].Texture.Y = imageRange.Top;
        _vertexBuffer.Buffer[1].Vector.X = clientRange.Right;
        _vertexBuffer.Buffer[1].Vector.Y = clientRange.Top;
        _vertexBuffer.Buffer[1].Texture.X = imageRange.Right;
        _vertexBuffer.Buffer[1].Texture.Y = imageRange.Top;
        _vertexBuffer.Buffer[2].Vector.X = clientRange.Left;
        _vertexBuffer.Buffer[2].Vector.Y = clientRange.Bottom;
        _vertexBuffer.Buffer[2].Texture.X = imageRange.Left;
        _vertexBuffer.Buffer[2].Texture.Y = imageRange.Bottom;
        _vertexBuffer.Buffer[3].Vector.X = clientRange.Right;
        _vertexBuffer.Buffer[3].Vector.Y = clientRange.Bottom;
        _vertexBuffer.Buffer[3].Texture.X = imageRange.Right;
        _vertexBuffer.Buffer[3].Texture.Y = imageRange.Bottom;
        _vertexBuffer.Flush();
        _graphics.Draw(4);
    }

    public void DrawFilterGraph(in RectF clientRange, float normalizedCutoff, float resonance)
    {
        var dumping = 1f - resonance;
        _graphBuffer.WriteByRef(new GraphParameters
        {
            X = normalizedCutoff,
            Y = dumping * dumping,
        });
        _graphics.PixelShader = _filterPixelShader;
        DrawImage(in clientRange, in _graphUvRect);
        _graphics.PixelShader = _imagePixelShader;
    }

    public void DrawCarrierGraph(in RectF clientRange, in GraphParameters parameters, in FmParameters fmParameters)
    {
        _graphBuffer.WriteByRef(in parameters);
        _fmBuffer.WriteByRef(in fmParameters);
        _graphics.PixelShader = _carrierPixelShader;
        DrawImage(in clientRange, in _graphUvRect);
        _graphics.PixelShader = _imagePixelShader;
    }

    public void DrawModulatorGraph(in RectF clientRange, in GraphParameters parameters)
    {
        _graphBuffer.WriteByRef(in parameters);
        _graphics.PixelShader = _modulatorPixelShader;
        DrawImage(in clientRange, in _graphUvRect);
        _graphics.PixelShader = _imagePixelShader;
    }

    public void Dispose()
    {
        _imagePixelShader.Dispose();
        _filterPixelShader.Dispose();
        _modulatorPixelShader.Dispose();
        _carrierPixelShader.Dispose();
        _graphics.Dispose();
    }
}
