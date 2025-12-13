using Tsumiki.Metadata;

namespace Tsumiki.Core;

/// <summary>
/// ユニゾン適用後の1つのオシレータ (Tsumiki は2つのオシレータを組み合わせて音を出す)
/// </summary>
internal struct StackedOscillator(StackConfig stackConfig, OscillatorConfig oscConfig, Modulation modulation, int oscillatorIndex)
{
    private readonly StackConfig _stackConfig = stackConfig;
    private readonly OscillatorConfig _oscConfig = oscConfig;
    private Stacked<ResetPulse> _pulses;
    private Stacked<OperatorWave> _carriers = new(new OperatorWave(oscConfig.CarrierWave));
    private Stacked<OperatorWave> _modulators = new(new OperatorWave(oscConfig.ModulatorWave));
    private Envelope _envelope1 = new(oscConfig.Envelope1);
    private Envelope _envelope2 = new(oscConfig.Envelope2);
    private readonly PitchModulation _pitchModulation = new(modulation.Config.PitchDest[oscillatorIndex], modulation);
    private readonly MultiplyModulation _levelModulation = new(modulation.Config.LevelDest[oscillatorIndex], modulation);
    private readonly MultiplyModulation _fmLevelModulation = new(modulation.Config.FmLevelDest[oscillatorIndex], modulation);
    private readonly AddModulation _panModulation = new(modulation.Config.PanDest[oscillatorIndex], modulation);

    public (float left, float right, float level) TickAndRender(bool noteOn, double voiceDelta)
    {
        if (_oscConfig.CarrierWave.Level == 0f) return default;

        var envelopeLevel = _envelope1.TickAndRender(noteOn);
        var level1 = envelopeLevel * (float)_levelModulation.Render();
        var level2 = _envelope2.TickAndRender(noteOn) * (float)_fmLevelModulation.Render();
        var modulatedDelta = voiceDelta * _pitchModulation.Render();
        var pan = _oscConfig.Pan + (float)_panModulation.Render();
        var left = 0f;
        var right = 0f;

        for (var i = 0; i < _stackConfig.Stack; i++)
        {
            var delta = modulatedDelta * _stackConfig.Pitches[i];
            var resetPhase = _pulses[i].Tick(delta);
            var fm = level2 * _modulators[i].TickAndRender(delta, resetPhase, 0f);
            var voiceOutput = level1 * _carriers[i].TickAndRender(delta, resetPhase, fm);
            var (lPan, rPan) = MathT.GetPanLevel(pan + _stackConfig.Pans[i]);
            left += voiceOutput * lPan;
            right += voiceOutput * rPan;
        }

        return (left, right, envelopeLevel);
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
