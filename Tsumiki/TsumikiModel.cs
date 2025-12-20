using System.Collections.Generic;
using NPlug;
using NPlug.IO;
using Tsumiki.Core;
using Tsumiki.Metadata;

namespace Tsumiki;

[VstModel("Tsumiki", typeof(ITsumikiModel))]
public partial class TsumikiModel
{
    public TsumikiController? Controller { get; set; }

    private static readonly AudioProgramListBuilder<TsumikiModel> DefaultProgramListBuilder = CreateAudioProgramListBuilder();

    private static AudioProgramListBuilder<TsumikiModel> CreateAudioProgramListBuilder()
    {
        return new AudioProgramListBuilder<TsumikiModel>("Bank")
        {
            model =>
            {
                InitModel(model);
                return new("Default");
            },
            model =>
            {
                InitModel(model);
                return new("Sine");
            },
        };
    }

    private static void InitModel(TsumikiModel model)
    {
        var parameterCount = model.ParameterCount;
        for (var i = 0; i < parameterCount; i++)
        {
            var parameter = model.GetParameterByIndex(i);
            parameter.NormalizedValue = parameter.DefaultNormalizedValue;
        }
    }

    private const ushort MaximumNormalParameterId = 999;
    private const ushort MinimumTuningParameterId = 1000;

    public override void Save(PortableBinaryWriter writer, AudioProcessorModelStorageMode mode)
    {
        TsumikiLogger.WriteAccess([mode]);
        try
        {
            var parameterCount = ParameterCount;
            var parameterList = new List<(ushort id, double value)>(parameterCount);
            var skipProgramChange = mode == AudioProcessorModelStorageMode.SkipProgramChangeParameters;
            var (minParameterId, maxParameterId) = SaveMode switch
            {
                SaveMode.TimbreOnly => (0, MaximumNormalParameterId),
                SaveMode.TuningOnly => (MinimumTuningParameterId, ushort.MaxValue),
                _ => (0, ushort.MaxValue),
            };

            for (var i = 0; i < parameterCount; i++)
            {
                var parameter = GetParameterByIndex(i);
                var parameterId = (ushort)parameter.Id.Value;
                if (minParameterId <= parameterId && parameterId < maxParameterId &&
                    (!skipProgramChange || !parameter.IsProgramChange))
                {
                    parameterList.Add(((ushort)parameter.Id.Value, parameter.NormalizedValue));
                }
            }

            writer.WriteUInt16((ushort)parameterList.Count);
            foreach (var (id, value) in parameterList)
            {
                writer.WriteUInt16(id);
                writer.WriteFloat64(value);
            }
        }
        catch (System.Exception ex)
        {
            TsumikiLogger.WriteException(ex);
        }
    }

    public override void Load(PortableBinaryReader reader, AudioProcessorModelStorageMode mode)
    {
        TsumikiLogger.WriteAccess([mode]);
        try
        {
            var count = reader.ReadUInt16();
            var skipProgramChange = mode == AudioProcessorModelStorageMode.SkipProgramChangeParameters;
            if (Controller is { } controller)
            {
                for (var i = 0; i < count; i++)
                {
                    var index = reader.ReadUInt16();
                    var value = reader.ReadFloat64();
                    if (TryGetParameterById(index, out var parameter) && (!skipProgramChange || !parameter.IsProgramChange))
                    {
                        controller.BeginEditParameter(parameter);
                        parameter.NormalizedValue = value;
                        controller.EndEditParameter();
                    }
                }
            }
            else
            {
                for (var i = 0; i < count; i++)
                {
                    var index = reader.ReadUInt16();
                    var value = reader.ReadFloat64();
                    if (TryGetParameterById(index, out var parameter) && (!skipProgramChange || !parameter.IsProgramChange))
                    {
                        parameter.NormalizedValue = value;
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            TsumikiLogger.WriteException(ex);
        }
    }
}
