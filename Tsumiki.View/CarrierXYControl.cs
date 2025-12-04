namespace Tsumiki.View;

internal class CarrierXYControl : XYControl<float, float>
{
    private readonly RectF _backgroundRect;
    private readonly IRangeViewParameter<double> _pitch;
    private readonly IViewParameter<bool> _sync;
    private readonly IViewParameter _level;
    private readonly IRangeViewParameter<float> _fmShapeX;
    private readonly IRangeViewParameter<float> _fmShapeY;
    private readonly IRangeViewParameter<double> _fmPitch;
    private readonly IViewParameter<bool> _fmSync;
    private readonly IRangeViewParameter<float> _fmLevel;

    public CarrierXYControl(
        IRangeViewParameter<float> shapeX,
        IRangeViewParameter<float> shapeY,
        IRangeViewParameter<double> pitch,
        IViewParameter<bool> sync,
        IViewParameter level,
        IRangeViewParameter<float> fmShapeX,
        IRangeViewParameter<float> fmShapeY,
        IRangeViewParameter<double> fmPitch,
        IViewParameter<bool> fmSync,
        IRangeViewParameter<float> fmLevel,
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
        _level = level;
        _fmShapeX = fmShapeX;
        _fmShapeY = fmShapeY;
        _fmPitch = fmPitch;
        _fmSync = fmSync;
        _fmLevel = fmLevel;
    }

    private const float MinX = 1f / 32768f;
    private const float MaxX = 1f - MinX;

    public override void OnParameterChanged(int parameterId)
    {
        if (parameterId != XData.Id &&
            parameterId != YData.Id &&
            parameterId != _pitch.Id &&
            parameterId != _sync.Id &&
            parameterId != _level.Id &&
            parameterId != _fmShapeX.Id &&
            parameterId != _fmShapeY.Id &&
            parameterId != _fmPitch.Id &&
            parameterId != _fmSync.Id &&
            parameterId != _fmLevel.Id)
            return;

        RequestRender();
    }

    internal override void RenderCore(IDrawingContext context)
    {
        var fmLevel = _fmLevel.Value;
        if (_level.NormalizedValue == 0.0)
        {
            // NOP
        }
        else if (fmLevel == 0f)
        {
            context.DrawModulatorGraph(_backgroundRect + GlobalRect.Location, new GraphParameters
            {
                X = Math.Clamp((float)XData.NormalizedValue, MinX, MaxX),
                Y = YData.Value,
                Pitch = (float)(_pitch.Value * 2.0),
                Period = _sync.Value ? 0.5f : 1f,
            });
        }
        else
        {
            context.DrawCarrierGraph(_backgroundRect + GlobalRect.Location, new GraphParameters
            {
                X = Math.Clamp((float)XData.NormalizedValue, MinX, MaxX),
                Y = YData.Value,
                Pitch = (float)(_pitch.Value * 2.0),
                Period = _sync.Value ? 0.5f : 1f,
            }, new FmParameters
            {
                X = Math.Clamp((float)_fmShapeX.NormalizedValue, MinX, MaxX),
                Y = _fmShapeY.Value,
                Pitch = (float)(_fmPitch.Value * 2.0),
                Period = _fmSync.Value ? 0.5f : 1f,
                Level = fmLevel * 0.5f,
            });
        }

        base.RenderCore(context);
    }
}
