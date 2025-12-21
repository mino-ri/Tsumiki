using Tsumiki.Core;

namespace Tsumiki.View;

public partial class TsumikiPage
{
    private static RectF VerticalSlider => PixelToTexture(1990, 1200, 90, 60);
    private static RectF HorizontalSlider => PixelToTexture(1930, 1110, 60, 90);
    private static RectF XYControl => PixelToTexture(1930, 1200, 60, 60);
    private static RectF NumberSwitcher => PixelToTexture(1930, 90, 90, 90);
    private static RectF SyncSwitcher => PixelToTexture(1930, 0, 90, 90);
    private static RectF StackTypeSwitcher => PixelToTexture(2110, 0, 90, 90);
    private static RectF DelayCrossSwitcher => PixelToTexture(2290, 0, 90, 90);
    private static RectF AttackSlider => PixelToTexture(1990, 1140, 60, 60);
    private static RectF ReleaseSlider => PixelToTexture(2050, 1140, 60, 60);
    private static RectF PitchDigit => PixelToTexture(1930, 360, 105, 90);
    private static RectF PitchDecimal => PixelToTexture(1930, 630, 25, 30);
    private static RectF LfoSpeedDecimal => PixelToTexture(1930, 180, 25, 30);
    private static RectF Attack => PixelToTexture(2230, 840, 120, 120);
    private static RectF Decay => PixelToTexture(2410, 840, 120, 120);
    private static RectF Under => PixelToTexture(2320, 930, 30, 30);
    private static RectF DelayBar => PixelToTexture(1930, 900, 30, 180);
    private static RectF DigitMiddle => PixelToTexture(2080, 0, 30, 60);

    public static TsumikiPage Create(ITsumikiViewModel data)
    {
        TsumikiPage page = [
            new TabPageControl(TabPageType.Main)
            {
                // Tab
                new TabSwitcher(TabPageType.Modulation, PixelToControl(150, 0, 150, 80), PixelToTexture(2260, 1000, 150, 80)),
                new TabSwitcher(TabPageType.Tuning, PixelToControl(300, 0, 150, 80), PixelToTexture(2410, 1000, 150, 80)),

                Carrier(PixelToControl(120, 80, 1560, 240), data.A1, data.A2),
                Modulator(PixelToControl(120, 320, 1560, 240), data.A2),
                Carrier(PixelToControl(120, 560, 1560, 240), data.B1, data.B2),
                Modulator(PixelToControl(120, 800, 1560, 240), data.B2),

                // Filter
                new FilterXYControl(data.Filter.Cutoff, data.Filter.Resonance, PixelToControl(120, 1040, 510, 240), XYControl),
                new VerticalSlider<float>(data.Filter.Mix, PixelToControl(630, 1040, 90, 240), VerticalSlider),

                // Delay
                new DelayPanel(data.Delay.Delay, data.Delay.Feedback, PixelToControl(840, 1040, 600, 240), PixelToControl(30, 30, 330, 180), DelayBar)
                {
                    new XYControl<int, float>(data.Delay.Delay, data.Delay.Feedback, PixelToControl(0, 0, 390, 240), XYControl),
                    new ToggleButton(data.Delay.Cross, PixelToControl(405, 15, 90, 90), DelayCrossSwitcher),
                    new VerticalSlider<float>(data.Delay.Mix, PixelToControl(510, 0, 90, 240), VerticalSlider),
                },

                // Input
                new VerticalSwitcher<int>(data.Input.Octave, PixelToControl(1815, 95, 90, 90), NumberSwitcher, 3, 0),
                new VerticalSwitcher<int>(data.Input.Bend, PixelToControl(1815, 215, 90, 90), NumberSwitcher, 3, 8),

                // Glide
                new GlideSlider(data.Input.Glide, PixelToControl(1815, 320, 90, 240), VerticalSlider),

                // Stack
                new VerticalSlider<float>(data.Master, PixelToControl(1575, 1040, 90, 240), VerticalSlider),
                new ToggleButton(data.Input.StackMode, PixelToControl(1815, 575, 90, 90), StackTypeSwitcher),
                new VerticalSwitcher<int>(data.Input.Stack, PixelToControl(1815, 695, 90, 90), NumberSwitcher, 3, 9),
                new VerticalSlider<int>(data.Input.StackDetune, PixelToControl(1815, 800, 90, 240), VerticalSlider),
                new VerticalSlider<float>(data.Input.StackStereo, PixelToControl(1815, 1040, 90, 240), VerticalSlider),
            },
            new TabPageControl(TabPageType.Modulation)
            {
                // Tab
                new TabSwitcher(TabPageType.Main, PixelToControl(0, 0, 150, 80), PixelToTexture(2110, 1000, 150, 80)),
                new TabSwitcher(TabPageType.Tuning, PixelToControl(300, 0, 150, 80), PixelToTexture(2410, 1000, 150, 80)),

                // Modulations
                Lfo(data.Modulation.Lfo),
                ModulationEnvelope(data.Modulation.Envelope),
                ModulationSource(PixelToControl(360, 480, 120, 800), PixelToTexture(1930, 240, 120, 160), data.Modulation.LfoSpeed),
                ModulationSource(PixelToControl(480, 480, 120, 800), PixelToTexture(1930, 240, 120, 160), data.Modulation.LfoLevel),
                ModulationSource(PixelToControl(600, 480, 120, 800), PixelToTexture(2080, 240, 120, 160), data.Modulation.APitch),
                ModulationSource(PixelToControl(720, 480, 120, 800), PixelToTexture(2080, 240, 120, 160), data.Modulation.APan),
                ModulationSource(PixelToControl(840, 480, 120, 800), PixelToTexture(2080, 240, 120, 160), data.Modulation.A1Level),
                ModulationSource(PixelToControl(960, 480, 120, 800), PixelToTexture(2230, 240, 120, 160), data.Modulation.A2Level),
                ModulationSource(PixelToControl(1080, 480, 120, 800), PixelToTexture(1930, 420, 120, 160), data.Modulation.BPitch),
                ModulationSource(PixelToControl(1200, 480, 120, 800), PixelToTexture(1930, 420, 120, 160), data.Modulation.BPan),
                ModulationSource(PixelToControl(1320, 480, 120, 800), PixelToTexture(1930, 420, 120, 160), data.Modulation.B1Level),
                ModulationSource(PixelToControl(1440, 480, 120, 800), PixelToTexture(2080, 420, 120, 160), data.Modulation.B2Level),
                ModulationSource(PixelToControl(1560, 480, 120, 800), PixelToTexture(2230, 420, 120, 160), data.Modulation.FilterCutoff),
                ModulationSource(PixelToControl(1680, 480, 120, 800), PixelToTexture(2230, 420, 120, 160), data.Modulation.FilterResonance),
                ModulationSource(PixelToControl(1800, 480, 120, 800), PixelToTexture(2230, 420, 120, 160), data.Modulation.FilterMix),
            },
            new TabPageControl(TabPageType.Tuning)
            {
                // Tab
                new TabSwitcher(TabPageType.Main, PixelToControl(0, 0, 150, 80), PixelToTexture(2110, 1000, 150, 80)),
                new TabSwitcher(TabPageType.Modulation, PixelToControl(150, 0, 150, 80), PixelToTexture(2260, 1000, 150, 80)),

                // Basic
                new DigitControl(data.Tuning.Root, PixelToControl(15, 140, 30, 60), DigitMiddle, 100),
                new DigitControl(data.Tuning.Root, PixelToControl(45, 140, 30, 60), DigitMiddle, 10),
                new DigitControl(data.Tuning.Root, PixelToControl(75, 140, 30, 60), DigitMiddle, 1),
                new DigitControl(data.Tuning.KeyPeriod, PixelToControl(135, 140, 30, 60), DigitMiddle, 100),
                new DigitControl(data.Tuning.KeyPeriod, PixelToControl(165, 140, 30, 60), DigitMiddle, 10),
                new DigitControl(data.Tuning.KeyPeriod, PixelToControl(195, 140, 30, 60), DigitMiddle, 1),

                // Hidden
                new VerticalSwitcher<SaveMode>(data.SaveMode, PixelToControl(270, 140, 60, 60), PixelToTexture(2080, 120, 60, 60), 1, 0),

                // View
                new ChannelNumber(PixelToControl(0, 230, 105, 105), PixelToTexture(360, 80, 105, 105), PixelToTexture(1140, 80, 105, 105), 150f / PixelTextureHeight, data.Tuning),
                new KeyBoardView(PixelToControl(180, 350, 150, 900), PixelToTexture(1930, 360, 150, 900), PixelToControl(0, -30), data.Tuning),
                new PitchBarView(PixelToControl(30, 350, 120, 900), PixelToTexture(2080, 960, 120, 30), PixelToTexture(2200, 960, 120, 30), data.Tuning),

                // Channels
                ChannelTuning(PixelToControl(480, 80 + 150 * 0, 660, 150), data.Tuning.Channel1),
                ChannelTuning(PixelToControl(480, 80 + 150 * 1, 660, 150), data.Tuning.Channel2),
                ChannelTuning(PixelToControl(480, 80 + 150 * 2, 660, 150), data.Tuning.Channel3),
                ChannelTuning(PixelToControl(480, 80 + 150 * 3, 660, 150), data.Tuning.Channel4),
                ChannelTuning(PixelToControl(480, 80 + 150 * 4, 660, 150), data.Tuning.Channel5),
                ChannelTuning(PixelToControl(480, 80 + 150 * 5, 660, 150), data.Tuning.Channel6),
                ChannelTuning(PixelToControl(480, 80 + 150 * 6, 660, 150), data.Tuning.Channel7),
                ChannelTuning(PixelToControl(480, 80 + 150 * 7, 660, 150), data.Tuning.Channel8),

                ChannelTuning(PixelToControl(1260, 80 + 150 * 0, 660, 150), data.Tuning.Channel9),
                ChannelTuning(PixelToControl(1260, 80 + 150 * 1, 660, 150), data.Tuning.Channel10),
                ChannelTuning(PixelToControl(1260, 80 + 150 * 2, 660, 150), data.Tuning.Channel11),
                ChannelTuning(PixelToControl(1260, 80 + 150 * 3, 660, 150), data.Tuning.Channel12),
                ChannelTuning(PixelToControl(1260, 80 + 150 * 4, 660, 150), data.Tuning.Channel13),
                ChannelTuning(PixelToControl(1260, 80 + 150 * 5, 660, 150), data.Tuning.Channel14),
                ChannelTuning(PixelToControl(1260, 80 + 150 * 6, 660, 150), data.Tuning.Channel15),
                ChannelTuning(PixelToControl(1260, 80 + 150 * 7, 660, 150), data.Tuning.Channel16),
            },
        ];

        page.SetTabPageType(TabPageType.Main);

        return page;
    }

    private static Panel Carrier(RectF rect, ICarrierViewModel carrier, IModulatorViewModel modulator)
    {
        return new(rect)
        {
            new PitchIntegerControl<double>(carrier.Pitch, PixelToControl(15, 60, 105, 90), PitchDigit, 3, 0, 16),
            new PitchDecimalControl<double>(carrier.Pitch, PixelToControl(35, 150, 25, 30), PitchDecimal, 16000, 16, 160),
            new PitchDecimalControl<double>(carrier.Pitch, PixelToControl(60, 150, 25, 30), PitchDecimal, 16000, 16, 1600),
            new PitchDecimalControl<double>(carrier.Pitch, PixelToControl(85, 150, 25, 30), PitchDecimal, 16000, 16, 16000),
            new ToggleButton(carrier.Sync, PixelToControl(135, 15, 90, 90), SyncSwitcher),
            new CarrierXYControl(
                carrier.ShapeX, carrier.ShapeY, carrier.Pitch, carrier.Sync, carrier.Level,
                modulator.ShapeX, modulator.ShapeY, modulator.Pitch, modulator.Sync, modulator.Level,
                PixelToControl(240, 0, 600, 240), XYControl),
            new HorizontalSlider<float>(carrier.Pan, PixelToControl(1320, 75, 240, 90), HorizontalSlider),
            new EnvelopePanel(carrier.Attack, carrier.Decay, carrier.Sustain, carrier.Release, carrier.Level,
                PixelToControl(840, 0, 480, 240), PixelToControl(30, 60, 360, 120), Attack, Decay, Under)
            {
                new VerticalSlider<float>(carrier.Level, PixelToControl(390, 0, 90, 240), VerticalSlider),
                new OffsetXYControl<int, float>(carrier.Attack, carrier.Decay, carrier.Sustain,
                    PixelToControlSize(120, 0), PixelToControl(0, 30, 300, 180), XYControl),
                new HorizontalSlider<int>(carrier.Attack, PixelToControl(0, 0, 180, 60), AttackSlider,
                    false, PixelControlWidth / 120.0),
                new HorizontalSlider<int>(carrier.Release, PixelToControl(240, 180, 180, 60), ReleaseSlider,
                    false, PixelControlWidth / 120.0),
            },
        };
    }

    private static Panel Modulator(RectF rect, IModulatorViewModel modulator)
    {
        return new(rect)
        {
            new PitchIntegerControl<double>(modulator.Pitch, PixelToControl(15, 60, 105, 90), PitchDigit, 3, 0, 16),
            new PitchDecimalControl<double>(modulator.Pitch, PixelToControl(35, 150, 25, 30), PitchDecimal, 16000, 16, 160),
            new PitchDecimalControl<double>(modulator.Pitch, PixelToControl(60, 150, 25, 30), PitchDecimal, 16000, 160, 1600),
            new PitchDecimalControl<double>(modulator.Pitch, PixelToControl(85, 150, 25, 30), PitchDecimal, 16000, 1600, 16000),
            new ToggleButton(modulator.Sync, PixelToControl(135, 15, 90, 90), SyncSwitcher),
            new ModulatorXYControl(modulator.ShapeX, modulator.ShapeY, modulator.Pitch, modulator.Sync, modulator.Level,
                PixelToControl(240, 0, 600, 240), XYControl),
            new EnvelopePanel(modulator.Attack, modulator.Decay, modulator.Sustain, modulator.Release, modulator.Level,
                PixelToControl(840, 0, 480, 240), PixelToControl(30, 60, 360, 120), Attack, Decay, Under)
            {
                new VerticalSlider<float>(modulator.Level, PixelToControl(390, 0, 90, 240), VerticalSlider),
                new OffsetXYControl<int, float>(modulator.Attack, modulator.Decay, modulator.Sustain,
                    PixelToControlSize(120, 0), PixelToControl(0, 30, 300, 180), XYControl),
                new HorizontalSlider<int>(modulator.Attack, PixelToControl(0, 0, 180, 60), AttackSlider,
                    false, PixelControlWidth / 120.0),
                new HorizontalSlider<int>(modulator.Release, PixelToControl(240, 180, 180, 60), ReleaseSlider,
                    false, PixelControlWidth / 120.0),
            },
        };
    }

    private static Panel Lfo(ILfoViewModel lfo)
    {
        return new(PixelToControl(120, 80, 840, 240))
        {
            new PitchDecimalControl<double>(lfo.Speed, PixelToControl(0, 75, 60, 60), PixelToTexture(1930, 15, 60, 60), 80000, 80, 8),
            new PitchDecimalControl<double>(lfo.Speed, PixelToControl(60, 75, 60, 60), PixelToTexture(1930, 105, 60, 60), 80000, 80, 80),
            new PitchDecimalControl<double>(lfo.Speed, PixelToControl(35, 150, 25, 30), LfoSpeedDecimal, 80000, 80, 800),
            new PitchDecimalControl<double>(lfo.Speed, PixelToControl(60, 150, 25, 30), LfoSpeedDecimal, 80000, 800, 8000),
            new PitchDecimalControl<double>(lfo.Speed, PixelToControl(85, 150, 25, 30), LfoSpeedDecimal, 80000, 8000, 80000),
            new ModulatorXYControl(lfo.ShapeX, lfo.ShapeY, null, null, lfo.Level,
                PixelToControl(150, 0, 600, 240), XYControl),
            new VerticalSlider<float>(lfo.Level, PixelToControl(750, 0, 90, 240), VerticalSlider),
        };
    }

    private static EnvelopePanel ModulationEnvelope(IModulationEnvelopeViewModel env)
    {
        return new(env.Attack, env.Decay, env.Sustain, env.Release, env.Level,
            PixelToControl(1080, 80, 840, 240), PixelToControl(30, 60, 720, 120),
            PixelToTexture(1930, 840, 240, 120), PixelToTexture(2200, 840, 240, 120), PixelToTexture(2170, 930, 30, 30))
        {
            new VerticalSlider<float>(env.Level, PixelToControl(750, 0, 90, 240), VerticalSlider),
            new OffsetXYControl<int, float>(env.Attack, env.Decay, env.Sustain,
                PixelToControlSize(240, 0), PixelToControl(0, 30, 540, 180), XYControl),
            new HorizontalSlider<int>(env.Attack, PixelToControl(0, 0, 300, 60), AttackSlider,
                false, PixelControlWidth / 240.0),
            new HorizontalSlider<int>(env.Release, PixelToControl(480, 180, 300, 60), ReleaseSlider,
                false, PixelControlWidth / 240.0),
        };
    }

    private static Panel ModulationSource(RectF control, RectF background, IModulationSourceViewModel source)
    {
        return new Panel(control)
        {
            new ModulationVerticalSlider(source.Lfo, PixelToControl(0, 0, 120, 160), background, VerticalSlider),
            new ModulationVerticalSlider(source.Env, PixelToControl(0, 160, 120, 160), background, VerticalSlider),
            new ModulationVerticalSlider(source.Wheel, PixelToControl(0, 320, 120, 160), background, VerticalSlider),
            new ModulationVerticalSlider(source.Velocity, PixelToControl(0, 480, 120, 160), background, VerticalSlider),
            new ModulationVerticalSlider(source.Pressure, PixelToControl(0, 640, 120, 160), background, VerticalSlider),
        };
    }

    private static Panel ChannelTuning(RectF control, IChannelTuningViewModel tuning)
    {
        return new Panel(control)
        {
            // Offset
            new DigitControl(tuning.Offset, PixelToControl(195, 60, 30, 60), DigitMiddle, 100),
            new DigitControl(tuning.Offset, PixelToControl(225, 60, 30, 60), DigitMiddle, 10),
            new DigitControl(tuning.Offset, PixelToControl(255, 60, 30, 60), DigitMiddle, 1),
            TuningValue(PixelToControl(15, 55, 165, 70), tuning.RatioN, tuning.RatioD, tuning.RatioPn, tuning.RatioPd),
            TuningValue(PixelToControl(300, 55, 165, 70), tuning.GeneratorN, tuning.GeneratorD, tuning.GeneratorPn, tuning.GeneratorPd),
            TuningValue(PixelToControl(480, 55, 165, 70), tuning.PeriodN, tuning.PeriodD, tuning.PeriodPn, tuning.PeriodPd),
        };
    }

    private static TuningValueControl TuningValue(RectF control, IRangeViewParameter<int> n, IRangeViewParameter<int> d, IRangeViewParameter<int> pn, IRangeViewParameter<int> pd)
    {
        return new TuningValueControl(control, n, d, pn, pd);
    }
}
