using Tsumiki.Metadata;

namespace Tsumiki.Core;

public class Processor()
{
    const int MaxVoices = 8;
    private MidiVoiceContainer _container = new(0);
    private StackedVoice[] _voices = [];
    private Delay _delay;
    private ConfigSet _config;
    private DelayConfig _delayConfig;

    public bool IsActive => _voices.Length > 0;

    [InitTiming]
    public void OnActive(bool isActive, ITsumikiModel model, double sampleRate)
    {
        if (isActive)
        {
            _container = new MidiVoiceContainer(MaxVoices);
            _delay = new Delay(sampleRate);
            _voices = GC.AllocateArray<StackedVoice>(MaxVoices, true);
            Recalculate(model, sampleRate);
        }
        else
        {
            _container = new(0);
            _delay = new(0);
            _voices = [];
        }
    }

    [EventTiming]
    public void Recalculate(ITsumikiModel model, double sampleRate)
    {
        _config.Recalculate(model, sampleRate);
        _delayConfig = new DelayConfig(model.Delay, sampleRate);
    }

    [EventTiming]
    public void ReserveNote(in MidiEventReservation<MidiNote> reservation)
    {
        _container.Reserve(in reservation);
    }

    [EventTiming]
    public void ProcessMain(ITsumikiModel model, double sampleRate, int sampleCount, Span<float> leftOutput, Span<float> rightOutput)
    {
        var masterVolume = model.Master;
        var delayMix = model.Delay.Mix;
        var delaySource = 1f - delayMix;

        var tickConfig = new TickConfig(model.PitchBend * model.Input.Bend, sampleRate, model.Filter.Mix);

        for (var sample = 0; sample < sampleCount; sample++)
        {
            var left = 0f;
            var right = 0f;
            _container.Tick();

            if (_config.VoceConfig.Polyphony)
            {
                for (var i = 0; i < MaxVoices; i++)
                {
                    var (l, r) = _voices[i].TickAndRender(in _config, in _container.Voices[i], in tickConfig, model);
                    left += l;
                    right += r;
                }
            }
            else
            {
                var (l, r) = _voices[0].TickAndRender(in _config, in _container.Voices[_container.Selector.LatestIndex], in tickConfig, model);
                left += l;
                right += r;
            }

            var (leftDelay, rightDelay) = _delay.TickAndRender(in _delayConfig, left, right);

            leftOutput[sample] = (left * delaySource + leftDelay * delayMix) * masterVolume;
            rightOutput[sample] = (right * delaySource + rightDelay * delayMix) * masterVolume;
        }
    }
}
