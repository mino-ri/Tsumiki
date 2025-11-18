using Tsumiki.Metadata;

namespace Tsumiki.Core;

/// <summary>
/// ユニゾン適用後の1つのオシレータ (Tsumiki は2つのオシレータを組み合わせて音を出す)
/// </summary>
internal struct StackedOscillator(StackConfig stackConfig, OscillatorConfig oscConfig)
{
    private readonly StackConfig _stackConfig = stackConfig;
    private readonly OscillatorConfig _oscConfig = oscConfig;
    private Stacked<ResetPulse> _pulses;
    private Stacked<CarrierWave> _carriers = new(new(oscConfig.CarrierWave));
    private Stacked<ModulatorWave> _modulators = new(new(oscConfig.ModulatorWave));
    private Envelope _envelope1 = new(oscConfig.Envelope1);
    private Envelope _envelope2 = new(oscConfig.Envelope2);

    public (float left, float right, float level) TickAndRender(bool noteOn, double voiceDelta)
    {
        var level1 = _envelope1.TickAndRender(noteOn);
        var level2 = _envelope2.TickAndRender(noteOn);
        var left = 0f;
        var right = 0f;
        for (var i = 0; i < _stackConfig.Stack; i++)
        {
            var delta = voiceDelta * _stackConfig.Pitches[i];
            var resetPhase = _pulses[i].Tick(delta);
            var fm = level2 * _modulators[i].TickAndRender(delta, resetPhase);
            var voiceOutput = level1 * _carriers[i].TickAndRender(delta, resetPhase, fm);
            var (lPan, rPan) = MathT.GetPanLevel(_oscConfig.Pan + _stackConfig.Pans[i]);
            left += voiceOutput * lPan;
            right += voiceOutput * rPan;
        }

        return (left, right, level1);
    }

    [EventTiming]
    public void StartNote()
    {
        for (var i = 0; i < _stackConfig.Stack; i++)
        {
            _carriers[i].Reset();
            _modulators[i].Reset();
            _pulses[i].Reset();
        }

        _envelope2.Reset();
    }

    [EventTiming]
    public void RestartNote()
    {
        _envelope1.Restart();
        _envelope2.Restart();
    }
}
