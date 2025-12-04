namespace Tsumiki.View;

internal class ModulatorXYControl : XYControl<float, float>
{
    private readonly RectF _backgroundRect;
    private readonly IRangeViewParameter<double> _pitch;
    private readonly IViewParameter<bool> _sync;

    public ModulatorXYControl(
        IRangeViewParameter<float> shapeX,
        IRangeViewParameter<float> shapeY,
        IRangeViewParameter<double> pitch,
        IViewParameter<bool> sync,
        RectF control,
        RectF texture)
        : base(shapeX, shapeY, control, texture)
    {
        var textureSize = TextureToControl(texture.Size);
        var motionSize = control.Size - textureSize;
        var origin = textureSize * 0.5f;
        _backgroundRect = new RectF(
            origin.Width,
            origin.Height,
            origin.Width + motionSize.Width,
            origin.Height + motionSize.Height);
        _pitch = pitch;
        _sync = sync;
    }

    private const float MinX = 1f / 32768f;
    private const float MaxX = 1f - MinX;

    public override void OnParameterChanged(int parameterId)
    {
        if (parameterId != XData.Id && parameterId != YData.Id && parameterId != _pitch.Id && parameterId != _sync.Id) return;

        RequestRender();
    }

    internal override void RenderCore(IDrawingContext context)
    {
        context.DrawModulatorGraph(_backgroundRect + GlobalRect.Location, new GraphParameters
        {
            X = Math.Clamp((float)XData.NormalizedValue, MinX, MaxX),
            Y = YData.Value,
            Pitch = (float)(_pitch.Value * 2.0),
            Period = _sync.Value ? 0.5f : 1f,
        });
        base.RenderCore(context);
    }
}
