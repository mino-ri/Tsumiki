using Tsumiki.Metadata;

namespace Tsumiki.Core;

/// <summary>
/// ユニゾン適用後の1つのオシレータ (Tsumiki は2つのオシレータを組み合わせて音を出す)
/// </summary>
internal struct StackedOscillator
{
    public Stacked<ResetPulse> Pulses;
    public Stacked<CarrierWave> Carriers;
    public Stacked<ModulatorWave> Modulators;
    public Envelope Envelope1;
    public Envelope Envelope2;

    public (float left, float right, float level) TickAndRender(in OscillatorConfig config, in StackConfig stackConfig, bool noteOn, double voiceDelta)
    {
        var level1 = Envelope1.TickAndRender(in config.Envelope1, noteOn);
        var level2 = Envelope2.TickAndRender(in config.Envelope2, noteOn);
        var left = 0f;
        var right = 0f;
        for (var i = 0; i < stackConfig.Stack; i++)
        {
            var delta = voiceDelta * stackConfig.Pitches[i];
            var resetPhase = Pulses[i].Tick(delta);
            var fm = level2 * Modulators[i].TickAndRender(in config.ModulatorWave, delta, resetPhase);
            var voiceOutput = level1 * Carriers[i].TickAndRender(in config.CarrierWave, delta, resetPhase, fm);
            var (lPan, rPan) = MathT.GetPanLevel(config.Pan + stackConfig.Pans[i]);
            left += voiceOutput * lPan;
            right += voiceOutput * rPan;
        }

        return (left, right, level1);
    }

    [EventTiming]
    public void StartNote(in OscillatorConfig config, in StackConfig stackConfig)
    {
        for (var i = 0; i < stackConfig.Stack; i++)
        {
            Carriers[i].Reset(in config.CarrierWave);
            Modulators[i].Reset(in config.ModulatorWave);
            Pulses[i].Reset();
        }

        Envelope2.Reset();
    }

    [EventTiming]
    public void RestartNote()
    {
        Envelope1.Restart();
        Envelope2.Restart();
    }
}
