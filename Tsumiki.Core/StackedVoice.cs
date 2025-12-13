using Tsumiki.Metadata;

namespace Tsumiki.Core;

/// <summary>
/// ユニゾン/倍音を統合した「スタック機能」の最大数だけ並んだ固定長配列。プログラミング用語の Stack とは無関係なので注意。
/// </summary>
[System.Runtime.CompilerServices.InlineArray(MathT.MaxStackCount)]
internal struct Stacked<T>
{
    private T _item;

#pragma warning disable CS8618
    public Stacked(T init)
#pragma warning restore CS8618
    {
        for (var i = 0; i < MathT.MaxStackCount; i++)
        {
            this[i] = init;
        }
    }

#pragma warning disable CS8618
    public Stacked(Func<T> init)
#pragma warning restore CS8618
    {
        for (var i = 0; i < MathT.MaxStackCount; i++)
        {
            this[i] = init();
        }
    }
}

[EventTiming]
internal class ConfigSet(ITsumikiModel model, double sampleRate)
{
    private readonly ITsumikiModel _model = model;
    public readonly DelayConfig Delay = new(model.Delay, sampleRate);
    public StackConfig Stack = new(model.Input);
    public GliderConfig Glide = new(model.Input, sampleRate);
    public ResonantLowPassFilterConfig[] Filter =
        [.. Enumerable.Range(0, MathT.MaxVoices).Select(_ => new ResonantLowPassFilterConfig(model.Filter, sampleRate))];
    public OscillatorConfig OscillatorA = new(model.A1, model.A2, sampleRate);
    public OscillatorConfig OscillatorB = new(model.B1, model.B2, sampleRate);
    public ModulationConfig Modulation = new(model, sampleRate);
    public float Master = model.Master;
    public double PitchBend = model.PitchBend * model.Input.Bend;
    public bool UseFilter = model.Filter.Mix != 0f;
    public float FilterMix = model.Filter.Mix;
    public float FilterSource = 1f - model.Filter.Mix;

    [EventTiming]
    public void Recalculate(double sampleRate)
    {
        Delay.Recalculate(sampleRate);
        Stack.Recalculate();
        Glide.Recalculate(sampleRate);
        for (var i = 0; i < MathT.MaxVoices; i++)
            Filter[i].Recalculate(sampleRate);
        OscillatorA.Recalculate(sampleRate);
        OscillatorB.Recalculate(sampleRate);
        Modulation.Recalculate(sampleRate);

        Master = _model.Master;
        PitchBend = _model.PitchBend * _model.Input.Bend;
        if (FilterMix != _model.Filter.Mix)
        {
            FilterMix = _model.Filter.Mix;
            UseFilter = FilterMix != 0f;
            FilterSource = 1f - FilterMix;
        }
    }
}

[EventTiming]
[method: InitTiming]
internal sealed class OscillatorConfig(ICarrierUnit carrierUnit, IModulatorUnit modulatorUnit, double sampleRate)
{
    private readonly ICarrierUnit _carrierUnit = carrierUnit;
    public OperatorWaveConfig CarrierWave = new(carrierUnit);
    public OperatorWaveConfig ModulatorWave = new(modulatorUnit);
    public EnvelopeConfig Envelope1 = new(carrierUnit, sampleRate);
    public EnvelopeConfig Envelope2 = new(modulatorUnit, sampleRate);
    public float Pan;

    [EventTiming]
    public void Recalculate(double sampleRate)
    {
        CarrierWave.Recalculate();
        ModulatorWave.Recalculate();
        Envelope1.Recalculate(sampleRate);
        Envelope2.Recalculate(sampleRate);
        Pan = _carrierUnit.Pan;
    }
}

[EventTiming]
[method: InitTiming]
internal class GliderConfig(IInputUnit unit, double sampleRate)
{
    private readonly IInputUnit _unit = unit;
    private int _glide;
    public double SampleRate = sampleRate;
    public bool Enable = unit.Glide > 0;
    public bool Polyphony = unit.Glide < 0;
    public int PitchShift = unit.Octave * 12;
    public FilterConfigD Filter = new(50 - unit.Glide, sampleRate);

    [EventTiming]
    public void Recalculate(double sampleRate)
    {
        var glide = _unit.Glide;
        if (_glide == glide && SampleRate == sampleRate) return;

        _glide = glide;
        SampleRate = sampleRate;

        Enable = _glide > 0;
        Polyphony = _glide < 0;
        if (_glide > 0)
        {
            Filter.Recalculate(50 - _glide, SampleRate);
        }

        PitchShift = _unit.Octave * 12;
    }
}

[EventTiming]
[method: InitTiming]
internal class StackConfig(IInputUnit unit)
{
    private static readonly int[][] DetuneFactors =
    [
        [],
        [ 0 ],
        [ -1, 1 ],
        [ 0, -4, 4 ],
        [ -1, 1, -9, 9 ],
        [ 0, -4, 4, -16, 16 ],
        [ -1, 1, -9, 9, -25, 25 ],
        [ 0, -4, 4, -16, 16, 36, -36 ],
    ];

    private static readonly int[][] PanFactors =
    [
        [],
        [ 0 ],
        [ -1, 1 ],
        [ 0, -2, 2 ],
        [ -1, 1, 3, -3 ],
        [ 0, -2, 2, 4, -4 ],
        [ -1, 1, 3, -3, -5, 5 ],
        [ 0, -2, 2, 4, -4, -6, 6 ],
    ];

    public int Stack;
    public Stacked<double> Pitches;
    public Stacked<float> Pans;
    private readonly IInputUnit _unit = unit;
    private StackMode _stackMode;
    private int _detune;
    private float _stereo;

    [EventTiming]
    public void Recalculate()
    {
        var recalculatePitch = Stack != _unit.Stack || _stackMode != _unit.StackMode || _detune != _unit.StackDetune;
        var recalculatePan = Stack != _unit.Stack || _stereo != _unit.StackStereo;

        Stack = _unit.Stack;
        _detune = _unit.StackDetune;
        _stackMode = _unit.StackMode;
        _stereo = _unit.StackStereo;

        if (recalculatePitch)
        {
            if (_stackMode == StackMode.Unison)
            {
                var factor = DetuneFactors[Stack];
                for (var i = 0; i < Stack; i++)
                {
                    Pitches[i] = Math.Pow(2.0, _detune * factor[i] / 48000.0);
                }
            }
            else
            {
                for (var i = 0; i < Stack; i++)
                {
                    Pitches[i] = i + 1;
                }
                Pitches[MathT.MaxStackCount - 1] = 8;
            }
        }

        if (recalculatePan)
        {
            if (_stereo == 0f)
            {
                ((Span<float>)Pans).Clear();
            }
            else
            {
                var factor = PanFactors[Stack];
                for (var i = 0; i < Stack; i++)
                {
                    Pans[i] = factor[i] * _stereo / Stack;
                }
            }
        }
    }
}

/// <summary>スタック機能を適用した後の音声出力器。</summary>
[EventTiming]
internal struct StackedVoice
{
    private readonly ConfigSet _config;
    private readonly Modulation _modulation;
    private readonly ResonantLowPassFilterConfig _filterConfig;
    private readonly MultiplyModulation _filterMixModulation;
    public SynthVoice SynthVoice;
    public StackedOscillator OscillatorA;
    public StackedOscillator OscillatorB;
    public ResonantLowPassFilter Filter;

    public StackedVoice(ConfigSet config, ResonantLowPassFilterConfig filterConfig, ModulationConfig modulationConfig)
    {
        _config = config;
        _filterConfig = filterConfig;
        _modulation = new(modulationConfig);
        _filterMixModulation = new(modulationConfig.FilterMixDest, _modulation);
        SynthVoice = new(config.Glide);
        OscillatorA = new(config.Stack, config.OscillatorA, _modulation, 0);
        OscillatorB = new(config.Stack, config.OscillatorB, _modulation, 1);
        Filter = new(filterConfig, _modulation);
    }

    [AudioTiming]
    public (float left, float right) TickAndRender(in MidiVoice midi)
    {
        switch (SynthVoice.Tick(in midi, _config.PitchBend))
        {
            case VoiceEvent.StartNote:
                // EVENT CALL
                StartNote();
                break;
            case VoiceEvent.RestartNote:
                // EVENT CALL
                RestartNote();
                break;
            case VoiceEvent.PitchChanged:
                _filterConfig.RecalculatePitch(SynthVoice.Pitch);
                break;
        }

        if (SynthVoice.State == VoiceState.Inactive)
            return default;
        
        var noteOn = SynthVoice.State == VoiceState.Active;
        _modulation.Tick(noteOn, SynthVoice.Velocity, SynthVoice.PolyPressure);
        var (leftA, rightA, levelA) = OscillatorA.TickAndRender(noteOn, SynthVoice.Delta);
        var (leftB, rightB, levelB) = OscillatorB.TickAndRender(noteOn, SynthVoice.Delta);
        if (SynthVoice.State == VoiceState.Release && levelA == 0f && levelB == 0f)
        {
            SynthVoice.State = VoiceState.Inactive;
            return default;
        }

        var left = (leftA + leftB) / _config.Stack.Stack;
        var right = (rightA + rightB) / _config.Stack.Stack;

        if (_config.UseFilter)
        {
            var filterMix = _config.FilterMix * (float)_filterMixModulation.Render();
            var (filteredLeft, filteredRight) = Filter.TickAndRender(left, right);
            return (_config.FilterSource * left + filterMix * filteredLeft,
                _config.FilterSource * right + filterMix * filteredRight);
        }
        else
        {
            return (left, right);
        }
    }

    [EventTiming]
    private void StartNote()
    {
        OscillatorA.StartNote();
        OscillatorB.StartNote();
        _modulation.StartNote();
        _filterConfig.RecalculatePitch(SynthVoice.Pitch);
        Filter.Reset();
    }

    [EventTiming]
    private void RestartNote()
    {
        OscillatorA.RestartNote();
        OscillatorB.RestartNote();
        _modulation.RestartNote();
        _filterConfig.RecalculatePitch(SynthVoice.Pitch);
    }
}
