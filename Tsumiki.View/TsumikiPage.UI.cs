using System.Threading;

namespace Tsumiki.View;

public partial class TsumikiPage
{
    static readonly RectF VerticalSlider = PixelToTexture(1990, 1200, 90, 60);
    static readonly RectF HorizontalSlider = PixelToTexture(1930, 1110, 60, 90);
    static readonly RectF XYControl = PixelToTexture(1930, 1200, 60, 60);
    static readonly RectF NumberSwitcher = PixelToTexture(1930, 90, 90, 90);
    static readonly RectF SyncSwitcher = PixelToTexture(1930, 0, 90, 90);
    static readonly RectF StackTypeSwitcher = PixelToTexture(2110, 0, 90, 90);
    static readonly RectF DelayCrossSwitcher = PixelToTexture(2290, 0, 90, 90);
    static readonly RectF AttackSlider = PixelToTexture(2080, 1200, 60, 60);
    static readonly RectF ReleaseSlider = PixelToTexture(2140, 1200, 60, 60);
    static readonly RectF PitchDigit = PixelToTexture(1930, 360, 105, 90);
    static readonly RectF PitchDecimal = PixelToTexture(1930, 630, 25, 30);

    static readonly RectF Attack = PixelToTexture(2230, 1140, 120, 120);
    static readonly RectF Decay = PixelToTexture(2410, 1140, 120, 120);
    static readonly RectF Under = PixelToTexture(2320, 1230, 30, 30);

    public static TsumikiPage Create(ITsumikiViewModel data)
    {

        return [
            Carrier(PixelToControl(120, 80, 1560, 240), data.A1),
            Modulator(PixelToControl(120, 320, 1560, 240), data.A2),
            Carrier(PixelToControl(120, 560, 1560, 240), data.B1),
            Modulator(PixelToControl(120, 800, 1560, 240), data.B2),

            // Filter
            new XYControl<int, float>(data.Filter.Cutoff, data.Filter.Resonance, PixelToControl(120, 1040, 510, 240), XYControl),
            new VerticalSlider<float>(data.Filter.Mix, PixelToControl(630, 1040, 90, 240), VerticalSlider),

            // Delay
            new XYControl<int, float>(data.Delay.Delay, data.Delay.Feedback, PixelToControl(840, 1040, 390, 240), XYControl),
            new ToggleButton(data.Delay.Cross, PixelToControl(1245, 1055, 90, 90), DelayCrossSwitcher),
            new VerticalSlider<float>(data.Delay.Mix, PixelToControl(1350, 1040, 90, 240), VerticalSlider),

            // Input
            new VerticalSwitcher<int>(data.Input.Octave, PixelToControl(1815, 95, 90, 90), NumberSwitcher, 3, 0),
            new VerticalSwitcher<int>(data.Input.Bend, PixelToControl(1815, 215, 90, 90), NumberSwitcher, 3, 8),

            // Glide
            new VerticalSlider<int>(data.Input.Glide, PixelToControl(1815, 320, 90, 240), VerticalSlider),

            // Stack
            new VerticalSlider<float>(data.Master, PixelToControl(1575, 1040, 90, 240), VerticalSlider),
            new ToggleButton(data.Input.StackMode, PixelToControl(1815, 575, 90, 90), StackTypeSwitcher),
            new VerticalSwitcher<int>(data.Input.Stack, PixelToControl(1815, 695, 90, 90), NumberSwitcher, 3, 9),
            new VerticalSlider<int>(data.Input.StackDetune, PixelToControl(1815, 800, 90, 240), VerticalSlider),
            new VerticalSlider<float>(data.Input.StackStereo, PixelToControl(1815, 1040, 90, 240), VerticalSlider),
        ];
    }

    private static Panel Carrier(RectF rect, ICarrierViewModel carrier)
    {
        return new(rect)
        {
            new PitchIntegerControl<double>(carrier.Pitch, PixelToControl(15, 60, 105, 90), PitchDigit, 3, 0, 16),
            new PitchDecimalControl<double>(carrier.Pitch, PixelToControl(35, 150, 25, 30), PitchDecimal, 16000, 160),
            new PitchDecimalControl<double>(carrier.Pitch, PixelToControl(60, 150, 25, 30), PitchDecimal, 16000, 1600),
            new PitchDecimalControl<double>(carrier.Pitch, PixelToControl(85, 150, 25, 30), PitchDecimal, 16000, 16000),
            new ToggleButton(carrier.Sync, PixelToControl(135, 15, 90, 90), SyncSwitcher),
            new XYControl<float, float>(carrier.ShapeX, carrier.ShapeY, PixelToControl(240, 0, 600, 240), XYControl),
            new HorizontalSlider<float>(carrier.Pan, PixelToControl(1320, 75, 240, 90), HorizontalSlider),
            new EnvelopePanel(carrier.Attack, carrier.Decay, carrier.Sustain, carrier.Release,
                PixelToControl(840, 0, 480, 240), PixelToControl(30, 60, 360, 120), Attack, Decay, Under)
            {
                new VerticalSlider<float>(carrier.Level, PixelToControl(390, 0, 90, 240), VerticalSlider),
                new OffsetXYControl<int, float>(carrier.Attack, carrier.Decay, carrier.Sustain, PixelToControlSize(120, 0), PixelToControl(0, 30, 300, 180), XYControl),
                new HorizontalSlider<int>(carrier.Attack, PixelToControl(0, 0, 180, 60), AttackSlider),
                new HorizontalSlider<int>(carrier.Release, PixelToControl(240, 180, 180, 60), ReleaseSlider),
            },
        };
    }

    private static Panel Modulator(RectF rect, IModulatorViewModel modulator)
    {
        return new(rect)
        {
            new PitchIntegerControl<double>(modulator.Pitch, PixelToControl(15, 60, 105, 90), PitchDigit, 3, 0, 16),
            new PitchDecimalControl<double>(modulator.Pitch, PixelToControl(35, 150, 25, 30), PitchDecimal, 16000, 160),
            new PitchDecimalControl<double>(modulator.Pitch, PixelToControl(60, 150, 25, 30), PitchDecimal, 16000, 1600),
            new PitchDecimalControl<double>(modulator.Pitch, PixelToControl(85, 150, 25, 30), PitchDecimal, 16000, 16000),
            new ToggleButton(modulator.Sync, PixelToControl(135, 15, 90, 90), SyncSwitcher),
            new XYControl<float, float>(modulator.ShapeX, modulator.ShapeY, PixelToControl(240, 0, 600, 240), XYControl),
            new VerticalSlider<float>(modulator.Level, PixelToControl(1230, 0, 90, 240), VerticalSlider),
            new EnvelopePanel(modulator.Attack, modulator.Decay, modulator.Sustain, modulator.Release,
                PixelToControl(840, 0, 480, 240), PixelToControl(30, 60, 360, 120), Attack, Decay, Under)
            {
                new VerticalSlider<float>(modulator.Level, PixelToControl(390, 0, 90, 240), VerticalSlider),
                new OffsetXYControl<int, float>(modulator.Attack, modulator.Decay, modulator.Sustain, PixelToControlSize(120, 0), PixelToControl(0, 30, 300, 180), XYControl),
                new HorizontalSlider<int>(modulator.Attack, PixelToControl(0, 0, 180, 60), AttackSlider),
                new HorizontalSlider<int>(modulator.Release, PixelToControl(240, 180, 180, 60), ReleaseSlider),
            },
        };
    }
}
