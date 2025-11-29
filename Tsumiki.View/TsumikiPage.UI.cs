using System.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Tsumiki.View;

public partial class TsumikiPage
{
    static readonly RectF VerticalSlider = PixelToTexture(1990, 1200, 90, 60);
    static readonly RectF HorizontalSlider = PixelToTexture(1930, 1110, 60, 90);
    static readonly RectF NumberSwitcher = PixelToTexture(1930, 90, 90, 90);
    static readonly RectF SyncSwitcher = PixelToTexture(1930, 0, 90, 90);
    static readonly RectF StackTypeSwitcher = PixelToTexture(2110, 0, 90, 90);
    static readonly RectF DelayCrossSwitcher = PixelToTexture(2290, 0, 90, 90);

    public static TsumikiPage Create(ITsumikiViewModel data)
    {

        return [
            Carrier(PixelToControl(120, 80, 1560, 240), data.A1),
            Modulator(PixelToControl(120, 320, 1560, 240), data.A2),
            Carrier(PixelToControl(120, 560, 1560, 240), data.B1),
            Modulator(PixelToControl(120, 800, 1560, 240), data.B2),

            // Filter
            new VerticalSlider<float>(data.Filter.Mix, PixelToControl(630, 1040, 90, 240), VerticalSlider),

            // Delay
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
            new ToggleButton(carrier.Sync, PixelToControl(135, 15, 90, 90), SyncSwitcher),
            new VerticalSlider<float>(carrier.Level, PixelToControl(1230, 0, 90, 240), VerticalSlider),
            new HorizontalSlider<float>(carrier.Pan, PixelToControl(1320, 75, 240, 90), HorizontalSlider),
        };
    }

    private static Panel Modulator(RectF rect, IModulatorViewModel modulator)
    {
        return new(rect)
        {
            new ToggleButton(modulator.Sync, PixelToControl(135, 15, 90, 90), SyncSwitcher),
            new VerticalSlider<float>(modulator.Level, PixelToControl(1230, 0, 90, 240), VerticalSlider),
        };
    }
}
