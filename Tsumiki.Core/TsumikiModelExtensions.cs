namespace Tsumiki.Core;

public static class TsumikiModelExtensions
{
    public static IChannelTuningUnit Channel(this ITuningUnit unit, int index)
    {
        return index switch
        {
            1 => unit.Channel2,
            2 => unit.Channel3,
            3 => unit.Channel4,
            4 => unit.Channel5,
            5 => unit.Channel6,
            6 => unit.Channel7,
            7 => unit.Channel8,
            8 => unit.Channel9,
            9 => unit.Channel10,
            10 => unit.Channel11,
            11 => unit.Channel12,
            12 => unit.Channel13,
            13 => unit.Channel14,
            14 => unit.Channel15,
            15 => unit.Channel16,
            _ => unit.Channel1,
        };
    }

    public static double GetPitch(this IChannelTuningUnit unit, int index)
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

    public static void SetPitch(this IChannelTuningUnit unit, int index, double value)
    {
        switch (index)
        {
            case 0: unit.Pitch000 = value; break;
            case 1: unit.Pitch001 = value; break;
            case 2: unit.Pitch002 = value; break;
            case 3: unit.Pitch003 = value; break;
            case 4: unit.Pitch004 = value; break;
            case 5: unit.Pitch005 = value; break;
            case 6: unit.Pitch006 = value; break;
            case 7: unit.Pitch007 = value; break;
            case 8: unit.Pitch008 = value; break;
            case 9: unit.Pitch009 = value; break;
            case 10: unit.Pitch010 = value; break;
            case 11: unit.Pitch011 = value; break;
            case 12: unit.Pitch012 = value; break;
            case 13: unit.Pitch013 = value; break;
            case 14: unit.Pitch014 = value; break;
            case 15: unit.Pitch015 = value; break;
            case 16: unit.Pitch016 = value; break;
            case 17: unit.Pitch017 = value; break;
            case 18: unit.Pitch018 = value; break;
            case 19: unit.Pitch019 = value; break;
            case 20: unit.Pitch020 = value; break;
            case 21: unit.Pitch021 = value; break;
            case 22: unit.Pitch022 = value; break;
            case 23: unit.Pitch023 = value; break;
            case 24: unit.Pitch024 = value; break;
            case 25: unit.Pitch025 = value; break;
            case 26: unit.Pitch026 = value; break;
            case 27: unit.Pitch027 = value; break;
            case 28: unit.Pitch028 = value; break;
            case 29: unit.Pitch029 = value; break;
            case 30: unit.Pitch030 = value; break;
            case 31: unit.Pitch031 = value; break;
            case 32: unit.Pitch032 = value; break;
            case 33: unit.Pitch033 = value; break;
            case 34: unit.Pitch034 = value; break;
            case 35: unit.Pitch035 = value; break;
            case 36: unit.Pitch036 = value; break;
            case 37: unit.Pitch037 = value; break;
            case 38: unit.Pitch038 = value; break;
            case 39: unit.Pitch039 = value; break;
            case 40: unit.Pitch040 = value; break;
            case 41: unit.Pitch041 = value; break;
            case 42: unit.Pitch042 = value; break;
            case 43: unit.Pitch043 = value; break;
            case 44: unit.Pitch044 = value; break;
            case 45: unit.Pitch045 = value; break;
            case 46: unit.Pitch046 = value; break;
            case 47: unit.Pitch047 = value; break;
            case 48: unit.Pitch048 = value; break;
            case 49: unit.Pitch049 = value; break;
            case 50: unit.Pitch050 = value; break;
            case 51: unit.Pitch051 = value; break;
            case 52: unit.Pitch052 = value; break;
            case 53: unit.Pitch053 = value; break;
            case 54: unit.Pitch054 = value; break;
            case 55: unit.Pitch055 = value; break;
            case 56: unit.Pitch056 = value; break;
            case 57: unit.Pitch057 = value; break;
            case 58: unit.Pitch058 = value; break;
            case 59: unit.Pitch059 = value; break;
            case 60: unit.Pitch060 = value; break;
            case 61: unit.Pitch061 = value; break;
            case 62: unit.Pitch062 = value; break;
            case 63: unit.Pitch063 = value; break;
            case 64: unit.Pitch064 = value; break;
            case 65: unit.Pitch065 = value; break;
            case 66: unit.Pitch066 = value; break;
            case 67: unit.Pitch067 = value; break;
            case 68: unit.Pitch068 = value; break;
            case 69: unit.Pitch069 = value; break;
            case 70: unit.Pitch070 = value; break;
            case 71: unit.Pitch071 = value; break;
            case 72: unit.Pitch072 = value; break;
            case 73: unit.Pitch073 = value; break;
            case 74: unit.Pitch074 = value; break;
            case 75: unit.Pitch075 = value; break;
            case 76: unit.Pitch076 = value; break;
            case 77: unit.Pitch077 = value; break;
            case 78: unit.Pitch078 = value; break;
            case 79: unit.Pitch079 = value; break;
            case 80: unit.Pitch080 = value; break;
            case 81: unit.Pitch081 = value; break;
            case 82: unit.Pitch082 = value; break;
            case 83: unit.Pitch083 = value; break;
            case 84: unit.Pitch084 = value; break;
            case 85: unit.Pitch085 = value; break;
            case 86: unit.Pitch086 = value; break;
            case 87: unit.Pitch087 = value; break;
            case 88: unit.Pitch088 = value; break;
            case 89: unit.Pitch089 = value; break;
            case 90: unit.Pitch090 = value; break;
            case 91: unit.Pitch091 = value; break;
            case 92: unit.Pitch092 = value; break;
            case 93: unit.Pitch093 = value; break;
            case 94: unit.Pitch094 = value; break;
            case 95: unit.Pitch095 = value; break;
            case 96: unit.Pitch096 = value; break;
            case 97: unit.Pitch097 = value; break;
            case 98: unit.Pitch098 = value; break;
            case 99: unit.Pitch099 = value; break;
            case 100: unit.Pitch100 = value; break;
            case 101: unit.Pitch101 = value; break;
            case 102: unit.Pitch102 = value; break;
            case 103: unit.Pitch103 = value; break;
            case 104: unit.Pitch104 = value; break;
            case 105: unit.Pitch105 = value; break;
            case 106: unit.Pitch106 = value; break;
            case 107: unit.Pitch107 = value; break;
            case 108: unit.Pitch108 = value; break;
            case 109: unit.Pitch109 = value; break;
            case 110: unit.Pitch110 = value; break;
            case 111: unit.Pitch111 = value; break;
            case 112: unit.Pitch112 = value; break;
            case 113: unit.Pitch113 = value; break;
            case 114: unit.Pitch114 = value; break;
            case 115: unit.Pitch115 = value; break;
            case 116: unit.Pitch116 = value; break;
            case 117: unit.Pitch117 = value; break;
            case 118: unit.Pitch118 = value; break;
            case 119: unit.Pitch119 = value; break;
            case 120: unit.Pitch120 = value; break;
            case 121: unit.Pitch121 = value; break;
            case 122: unit.Pitch122 = value; break;
            case 123: unit.Pitch123 = value; break;
            case 124: unit.Pitch124 = value; break;
            case 125: unit.Pitch125 = value; break;
            case 126: unit.Pitch126 = value; break;
            default: unit.Pitch127 = value; break;
        }
    }

    public static void RebuildPitches(this IChannelTuningUnit unit, int rootIndex, int keyCount)
    {
        var offset = unit.Offset;
        var ratio = GetPitchValue(unit.RatioN, unit.RatioD, unit.RatioPn, unit.RatioPd);
        var generator = GetPitchValue(unit.GeneratorN, unit.GeneratorD, unit.GeneratorPn, unit.GeneratorPd);
        var period = GetPitchValue(unit.PeriodN, unit.PeriodD, unit.PeriodPn, unit.PeriodPd);
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

            unit.SetPitch(i, period * periodCount + periodPitches[index]);
        }
    }

    private static double GetPitchValue(int n, int d, int pn, int pd)
    {
        var baseValue = Math.Log2((double)n / d) * 12;
        return baseValue * pn / pd;
    }
}
