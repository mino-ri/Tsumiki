using System.Threading.Channels;

namespace Tsumiki.View;

internal class KeyBoardView : Control
{
    private readonly ITuningViewModel _tuning;
    private readonly RectF[] _keyImages;

    public KeyBoardView(RectF control, RectF keyImage0, PointF keyImageOffset, ITuningViewModel tuning) : base(control)
    {
        _tuning = tuning;
        _keyImages = new RectF[12];
        for (int i = 0; i < 12; i++)
        {
            _keyImages[i] = keyImage0 + keyImageOffset * i;
        }
    }

    internal override void RenderCore(IDrawingContext context)
    {
        var minimum = Math.Min(_tuning.Root.Value, 127 - 29);

        context.DrawImage(in GlobalRect, in _keyImages[minimum % 12]);
    }

    public override void OnParameterChanged(int parameterId)
    {
        if (parameterId == _tuning.Root.Id || parameterId == _tuning.KeyPeriod.Id)
        {
            RequestRender();
        }
    }

    internal override bool TryFindParameter(PointF point, out int parameterId)
    {
        parameterId = 0;
        return false;
    }
}

internal class PitchBarView(RectF control, RectF lineImage, RectF rootLineImage, ITuningViewModel tuning) : Control(control)
{
    private bool _shouldRecalculate = true;
    private IChannelTuningViewModel _channel = tuning.Channel1;
    private readonly double[] _pitches = new double[30];
    private readonly SizeF _foregroundSize = TextureToControl(lineImage.Size);
    private readonly float _motionHeight = control.Height - TextureToControl(lineImage.Size).Height;

    internal override void RenderCore(IDrawingContext context)
    {
        var root = tuning.Root.Value;
        var keyPeriod = tuning.KeyPeriod.Value;
        var minimum = Math.Min(root, 127 - 29);
        var maximum = minimum + 29;

        if (_shouldRecalculate)
        {
            _shouldRecalculate = false;
            var channel = _channel;
            if (channel.IsCustomPitch.Value)
            {
                for (var key = minimum; key <= maximum; key++)
                {
                    _pitches[key - minimum] = channel.Pitch(key).Value;
                }
            }
            else
            {
                var ratio = Updated(channel.RatioN.Value, channel.RatioD.Value, channel.RatioPn.Value, channel.RatioPd.Value);
                var generator = Updated(channel.GeneratorN.Value, channel.GeneratorD.Value, channel.GeneratorPn.Value, channel.GeneratorPd.Value);
                var period = Updated(channel.PeriodN.Value, channel.PeriodD.Value, channel.PeriodPn.Value, channel.PeriodPd.Value);
                var offset = channel.Offset.Value;
                var basePitch = root + ratio;

                // KeyCount は 1～127 の範囲なので常に stack alloc が可能
                Span<double> periodPitches = stackalloc double[keyPeriod];
                for (var i = 0; i < keyPeriod; i++)
                {
                    var factor = i - offset;
                    var targetPitch = generator * factor % period;
                    if (factor < 0 && targetPitch != 0.0)
                    {
                        targetPitch += period;
                    }

                    targetPitch += basePitch;
                    periodPitches[i] = targetPitch;
                }

                periodPitches.Sort();

                for (var key = minimum; key <= maximum; key++)
                {
                    var relativeKey = key - root;
                    var (periodCount, index) = Math.DivRem(relativeKey, keyPeriod);
                    if (relativeKey < 0 && index != 0)
                    {
                        periodCount--;
                        index += keyPeriod;
                    }

                    _pitches[key - minimum] = Math.Clamp(period * periodCount + periodPitches[index], 0.0, 128.0);
                }
            }
        }

        for (var key = minimum; key <= maximum; key++)
        {
            var isRoot = keyPeriod == 1 ? key == root : (key - root) % keyPeriod == 0;
            RenderPitch(context, _pitches[key - minimum], minimum, isRoot);
        }
    }

    private void RenderPitch(IDrawingContext context, double pitch, int minimum, bool isRoot)
    {
        var relativePitch = 29.0 - (pitch - minimum);
        if (relativePitch is < 0.0 or > 29.0) return;

        var foregroundTop = (float)relativePitch * _motionHeight / 29f;
        var controlRect = new RectF(
            GlobalRect.Left,
            GlobalRect.Top + foregroundTop,
            GlobalRect.Right,
            GlobalRect.Top + _foregroundSize.Height + foregroundTop);

        if (isRoot)
        {
            context.DrawImage(in controlRect, in rootLineImage);
        }
        else
        {
            context.DrawImage(in controlRect, in lineImage);
        }
    }

    public override void OnParameterChanged(int parameterId)
    {
        if (parameterId == tuning.Root.Id || parameterId == tuning.KeyPeriod.Id)
        {
            _shouldRecalculate = true;
            RequestRender();
        }

        for (var i = 0; i < 16; i++)
        {
            var channel = tuning.Channel(i);
            if (channel.Offset.Id <= parameterId && parameterId <= channel.Pitch127.Id)
            {
                _channel = channel;
                _shouldRecalculate = true;

                RequestRender();
            }
        }
    }

    internal override bool TryFindParameter(PointF point, out int parameterId)
    {
        parameterId = 0;
        return false;
    }

    private static double Updated(int n, int d, int pn, int pd) => Math.Log2((double)n / d) * 12.0 * pn / pd;

}

internal class ChannelNumber(RectF control, RectF firstImage, RectF secondImage, float offset, ITuningViewModel tuning) : Control(control)
{
    private int _index = 0;

    public override void OnParameterChanged(int parameterId)
    {
        for (var i = 0; i < 16; i++)
        {
            var channel = tuning.Channel(i);
            if (channel.Offset.Id <= parameterId && parameterId <= channel.Pitch127.Id)
            {
                _index = i;
                RequestRender();
            }
        }
    }

    internal override void RenderCore(IDrawingContext context)
    {
        if (_index < 8)
        {
            context.DrawImage(in GlobalRect, firstImage + new PointF(0f, offset * _index));
        }
        else
        {
            context.DrawImage(in GlobalRect, secondImage + new PointF(0f, offset * (_index - 8)));
        }
    }

    internal override bool TryFindParameter(PointF point, out int parameterId)
    {
        parameterId = 0;
        return false;
    }
}
