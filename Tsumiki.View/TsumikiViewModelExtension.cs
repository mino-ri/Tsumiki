namespace Tsumiki.View;

public static class TsumikiViewModelExtension
{
    public static IRangeViewParameter<double> Pitch(this IChannelTuningViewModel unit, int index)
    {
        return index switch
        {
            0 => unit.Pitch000,
            1 => unit.Pitch001,
            2 => unit.Pitch002,
            3 => unit.Pitch003,
            4 => unit.Pitch004,
            5 => unit.Pitch005,
            6 => unit.Pitch006,
            7 => unit.Pitch007,
            8 => unit.Pitch008,
            9 => unit.Pitch009,
            10 => unit.Pitch010,
            11 => unit.Pitch011,
            12 => unit.Pitch012,
            13 => unit.Pitch013,
            14 => unit.Pitch014,
            15 => unit.Pitch015,
            16 => unit.Pitch016,
            17 => unit.Pitch017,
            18 => unit.Pitch018,
            19 => unit.Pitch019,
            20 => unit.Pitch020,
            21 => unit.Pitch021,
            22 => unit.Pitch022,
            23 => unit.Pitch023,
            24 => unit.Pitch024,
            25 => unit.Pitch025,
            26 => unit.Pitch026,
            27 => unit.Pitch027,
            28 => unit.Pitch028,
            29 => unit.Pitch029,
            30 => unit.Pitch030,
            31 => unit.Pitch031,
            32 => unit.Pitch032,
            33 => unit.Pitch033,
            34 => unit.Pitch034,
            35 => unit.Pitch035,
            36 => unit.Pitch036,
            37 => unit.Pitch037,
            38 => unit.Pitch038,
            39 => unit.Pitch039,
            40 => unit.Pitch040,
            41 => unit.Pitch041,
            42 => unit.Pitch042,
            43 => unit.Pitch043,
            44 => unit.Pitch044,
            45 => unit.Pitch045,
            46 => unit.Pitch046,
            47 => unit.Pitch047,
            48 => unit.Pitch048,
            49 => unit.Pitch049,
            50 => unit.Pitch050,
            51 => unit.Pitch051,
            52 => unit.Pitch052,
            53 => unit.Pitch053,
            54 => unit.Pitch054,
            55 => unit.Pitch055,
            56 => unit.Pitch056,
            57 => unit.Pitch057,
            58 => unit.Pitch058,
            59 => unit.Pitch059,
            60 => unit.Pitch060,
            61 => unit.Pitch061,
            62 => unit.Pitch062,
            63 => unit.Pitch063,
            64 => unit.Pitch064,
            65 => unit.Pitch065,
            66 => unit.Pitch066,
            67 => unit.Pitch067,
            68 => unit.Pitch068,
            69 => unit.Pitch069,
            70 => unit.Pitch070,
            71 => unit.Pitch071,
            72 => unit.Pitch072,
            73 => unit.Pitch073,
            74 => unit.Pitch074,
            75 => unit.Pitch075,
            76 => unit.Pitch076,
            77 => unit.Pitch077,
            78 => unit.Pitch078,
            79 => unit.Pitch079,
            80 => unit.Pitch080,
            81 => unit.Pitch081,
            82 => unit.Pitch082,
            83 => unit.Pitch083,
            84 => unit.Pitch084,
            85 => unit.Pitch085,
            86 => unit.Pitch086,
            87 => unit.Pitch087,
            88 => unit.Pitch088,
            89 => unit.Pitch089,
            90 => unit.Pitch090,
            91 => unit.Pitch091,
            92 => unit.Pitch092,
            93 => unit.Pitch093,
            94 => unit.Pitch094,
            95 => unit.Pitch095,
            96 => unit.Pitch096,
            97 => unit.Pitch097,
            98 => unit.Pitch098,
            99 => unit.Pitch099,
            100 => unit.Pitch100,
            101 => unit.Pitch101,
            102 => unit.Pitch102,
            103 => unit.Pitch103,
            104 => unit.Pitch104,
            105 => unit.Pitch105,
            106 => unit.Pitch106,
            107 => unit.Pitch107,
            108 => unit.Pitch108,
            109 => unit.Pitch109,
            110 => unit.Pitch110,
            111 => unit.Pitch111,
            112 => unit.Pitch112,
            113 => unit.Pitch113,
            114 => unit.Pitch114,
            115 => unit.Pitch115,
            116 => unit.Pitch116,
            117 => unit.Pitch117,
            118 => unit.Pitch118,
            119 => unit.Pitch119,
            120 => unit.Pitch120,
            121 => unit.Pitch121,
            122 => unit.Pitch122,
            123 => unit.Pitch123,
            124 => unit.Pitch124,
            125 => unit.Pitch125,
            126 => unit.Pitch126,
            _ => unit.Pitch127,
        };
    }

    public static void RebuildPitches(this IChannelTuningViewModel unit, int rootIndex, int keyCount)
    {
        var offset = unit.Offset.Value;
        var ratio = GetPitchValue(unit.RatioN.Value, unit.RatioD.Value, unit.RatioPn.Value, unit.RatioPd.Value);
        var generator = GetPitchValue(unit.GeneratorN.Value, unit.GeneratorD.Value, unit.GeneratorPn.Value, unit.GeneratorPd.Value);
        var period = GetPitchValue(unit.PeriodN.Value, unit.PeriodD.Value, unit.PeriodPn.Value, unit.PeriodPd.Value);
        var basePitch = rootIndex + ratio;

        // KeyCount は 1～127 の範囲なので常に stack alloc が可能
        Span<double> periodPitches = stackalloc double[keyCount];
        for (var i = 0; i < keyCount; i++)
        {
            var factor = i - offset;
            var targetPitch = generator * factor % period;
            if (factor < 0 && targetPitch != 0.0)
            {
                targetPitch += period;
            }

            targetPitch += basePitch;
            periodPitches[i] = targetPitch;
        }

        periodPitches.Sort();

        for (var i = 0; i < 128; i++)
        {
            var relativeKey = i - rootIndex;
            var (periodCount, index) = Math.DivRem(relativeKey, keyCount);
            if (relativeKey < 0 && index != 0)
            {
                periodCount--;
                index += keyCount;
            }

            var pitchViewModel = unit.Pitch(i);
            pitchViewModel.BeginEdit();
            pitchViewModel.Value = period * periodCount + periodPitches[index];
            pitchViewModel.EndEdit();
        }
    }

    private static double GetPitchValue(int n, int d, int pn, int pd)
    {
        var baseValue = Math.Log2((double)n / d) * 12;
        return baseValue * pn / pd;
    }
}
