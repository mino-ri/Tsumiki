using Tsumiki.Metadata;

namespace Tsumiki.Core;

public class Processor
{
    const int MaxVoices = 8;
    private readonly ITsumikiModel _model;
    private readonly ConfigSet _config;
    private double _sampleRate;
    private MidiVoiceContainer _container;
    private StackedVoice[] _voices;
    private Delay _delay;

    public bool IsActive => _voices.Length > 0;

    [InitTiming]
    public Processor(ITsumikiModel model, double sampleRate)
    {
        _model = model;
        _config = new(model, sampleRate);
        _sampleRate = sampleRate;
        _container = new(0);
        _voices = [];
        _delay = new(_config.Delay, sampleRate);
    }

    [InitTiming]
    public void OnActive(bool isActive)
    {
        if (isActive)
        {
            _container = new MidiVoiceContainer(MaxVoices);
            _voices = GC.AllocateArray<StackedVoice>(MaxVoices, true);
            for (var i = 0; i < MaxVoices; i++)
            {
                _voices[i] = new(_config, _config.Filter[i]);
            }
            Recalculate();
        }
        else
        {
            _container = new(0);
            _voices = [];
        }
    }

    [EventTiming]
    public void ChangeSampleRate(double sampleRate)
    {
        _sampleRate = sampleRate;
        Recalculate();
    }

    [EventTiming]
    public void Recalculate()
    {
        _config.Recalculate(_sampleRate);
    }

    [EventTiming]
    public void ReserveNote(in MidiEventReservation<MidiNote> reservation)
    {
        _container.Reserve(in reservation);
    }

    [EventTiming]
    public void ProcessMain(int sampleCount, Span<float> leftOutput, Span<float> rightOutput)
    {
        var masterVolume = _config.Master;
        var delayMix = _config.Delay.Mix;
        var delaySource = 1f - delayMix;

        for (var sample = 0; sample < sampleCount; sample++)
        {
            var left = 0f;
            var right = 0f;
            _container.Tick();

            if (_config.Glide.Polyphony)
            {
                for (var i = 0; i < MaxVoices; i++)
                {
                    var (l, r) = _voices[i].TickAndRender(in _container.Voices[i]);
                    left += l;
                    right += r;
                }
            }
            else
            {
                var (l, r) = _voices[0].TickAndRender(in _container.Voices[_container.Selector.LatestIndex]);
                left += l;
                right += r;
            }

            var (leftDelay, rightDelay) = _delay.TickAndRender(left, right);

            leftOutput[sample] = (left * delaySource + leftDelay * delayMix) * masterVolume;
            rightOutput[sample] = (right * delaySource + rightDelay * delayMix) * masterVolume;
        }
    }
}
