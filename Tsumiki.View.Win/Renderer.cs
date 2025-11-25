using System.Numerics;
using System.Runtime.InteropServices;
using IndirectX;
using IndirectX.D3D11;
using IndirectX.Dxgi;
using IndirectX.Helper;

namespace Tsumiki.View.Win;

internal sealed class Renderer : IDisposable
{
    [StructLayout(LayoutKind.Sequential)]
    private struct ConstantBuffer
    {
        public Vector4 Scale;
        public Vector4 Location;
    }

    private const int syncInterval = 2;
    private readonly Graphics _graphics;
    private readonly IResourceTexture _sourceTexture;
    private readonly Vertex[] _vertices;
    private readonly ushort[] _indices;
    private readonly ArrayBuffer<Vertex> _vertexBuffer;
    private (int width, int height)? _changedSize;

    public static WindowMessage Message { get; set; }

    public Renderer(nint hwnd, int width, int height)
    {
        _graphics = new Graphics(hwnd, width, height,
            windowed: true,
            refreshRate: 60,
            syncInterval: syncInterval,
            useZBuffer: false);
        _vertices =
        [
            new Vertex(0f, 0f, 0.5f, 1f, Color.White, new Vector2(0f, 0f)),
            new Vertex(1f, 0f, 0.5f, 1f, Color.White, new Vector2(0.75f, 0f)),
            new Vertex(0f, 1f, 0.5f, 1f, Color.White, new Vector2(0f, 1f)),
            new Vertex(1f, 1f, 0.5f, 1f, Color.White, new Vector2(0.75f, 1f)),
        ];

        _indices = [0, 1, 2, 2, 1, 3];

        _graphics.SetVertexShader(ShaderSource.LoadVertexShader);
        _graphics.SetPixelShader(ShaderSource.LoadPixelShader);
        _graphics.SetInputLayout(ShaderSource.LoadInputLayout,
            new InputElementDesc { SemanticName = "POSITION", Format = Format.R32G32B32A32Float },
            new InputElementDesc { SemanticName = "COLOR", Format = Format.R32G32B32A32Float, AlignedByteOffset = 16 },
            new InputElementDesc { SemanticName = "TEXCOORD", Format = Format.R32G32Float, AlignedByteOffset = 32 });

        _vertexBuffer = _graphics.RegisterVertexBuffer<Vertex>(0, 4);
        _graphics.RegisterConstantBuffer<ConstantBuffer>(0, ShaderStages.VertexShader).WriteByRef(
            new ConstantBuffer
            {
                Scale = new Vector4(2f, -2f, 1f, 1f),
                Location = new Vector4(-1f, 1f, 0f, 0f),
            });

        _graphics.RegisterIndexBuffer(6)
                .Write(_indices);

        _sourceTexture = Resources.ImageResource.LoadMain(_graphics);
        _graphics.SetTexture(0, _sourceTexture);
        _graphics.SetBorderSampler(0);
        using var alphaBlendState = _graphics.CreateAlphaBlendState();
        _graphics.SetBlendState(alphaBlendState);
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
            }

            _graphics.ClearRenderTarget(new Color(0xFF504530));
            _vertexBuffer.Write(_vertices);
            _graphics.DrawIndexed(6);
            _graphics.SwapChain.TryPresent(syncInterval, PresentFlags.None);
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

    public void Dispose() => _graphics.Dispose();
}
