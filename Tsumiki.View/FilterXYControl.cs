namespace Tsumiki.View;

internal class FilterXYControl : XYControl<int, float>
{
    private readonly RectF _backgroundRect;

    public FilterXYControl(
        IParameterGroup parameterGroup,
        IRangeViewParameter<int> cutoff,
        IRangeViewParameter<float> resonance,
        RectF control,
        RectF texture)
        : base(parameterGroup, cutoff, resonance, control, texture)
    {
        var textureSize = TextureToControl(texture.Size);
        var motionSize = control.Size - textureSize;
        var origin = textureSize * 0.5f;
        _backgroundRect = new RectF(
            origin.Width,
            origin.Height,
            origin.Width + motionSize.Width,
            origin.Height + motionSize.Height);
    }

    internal override void RenderCore(IDrawingContext context)
    {
        context.DrawFilterGraph(_backgroundRect + GlobalRect.Location, (float)XData.NormalizedValue, YData.Value);
        base.RenderCore(context);
    }
}
