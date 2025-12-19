using Tsumiki.Core;

namespace Tsumiki.Test.Core;

public static class ChannelTuningConfigTest
{
    [Fact]
    public static void RebuildPitches_12平均律()
    {
        var tuningUnit = new TuningTuningUnit
        {
            Root = 69,
            KeyPeriod = 12,
        };
        var unit = new Channel1ChannelTuningUnit
        {
            Offset = 4,
            RatioN = 1,
            RatioD = 1,
            RatioPn = 0,
            RatioPd = 1,
            PeriodN = 2,
            PeriodD = 1,
            PeriodPn = 1,
            PeriodPd = 1,
            GeneratorN = 2,
            GeneratorD = 1,
            GeneratorPn = 1,
            GeneratorPd = 12,
        };

        var config = new ChannelTuningConfig(tuningUnit, unit);
        unit.Offset = 5;
        config.Recalculate();

        for (var i = 0; i < 128; i++)
        {
            var expectedPitch = (double)i;
            var actualPitch = unit.GetPitch(i);
            
            Assert.Equal(expectedPitch, actualPitch, 1e-9);
        }
    }

    [Fact]
    public static void RebuildPitches_31平均律()
    {
        var tuningUnit = new TuningTuningUnit
        {
            Root = 69,
            KeyPeriod = 12,
        };
        var unit = new Channel1ChannelTuningUnit
        {
            Offset = 4,
            RatioN = 1,
            RatioD = 1,
            RatioPn = 0,
            RatioPd = 1,
            PeriodN = 2,
            PeriodD = 1,
            PeriodPn = 1,
            PeriodPd = 1,
            GeneratorN = 2,
            GeneratorD = 1,
            GeneratorPn = 18,
            GeneratorPd = 31,
        };

        var config = new ChannelTuningConfig(tuningUnit, unit);
        unit.Offset = 5;
        config.Recalculate();

        for (var i = 0; i < 128; i++)
        {
            var expectedPitch = (double)i;
            var actualPitch = unit.GetPitch(i);

            Assert.Equal(expectedPitch, actualPitch, 0.25);
        }
    }
}
