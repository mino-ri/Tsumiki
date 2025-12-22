using System;
using System.Collections.Generic;
using System.IO;
using NPlug;
using NPlug.IO;
using Tsumiki.Core;
using Tsumiki.Metadata;

namespace Tsumiki;

[VstModel("Tsumiki", typeof(ITsumikiModel))]
public partial class TsumikiModel
{
    private int _threadId;

    public TsumikiController? Controller
    {
        get;
        set
        {
            field = value;
            _threadId = Environment.CurrentManagedThreadId;
        }
    }

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
        var currentThreadId = Environment.CurrentManagedThreadId;
        TsumikiLogger.WriteAccess([mode, _threadId, currentThreadId]);
        try
        {
            var count = reader.ReadUInt16();
            var valueDictionary = new Dictionary<ushort, double>();
            for (var i = 0; i < count; i++)
            {
                var index = reader.ReadUInt16();
                var value = reader.ReadFloat64();
                valueDictionary[index] = value;
            }

            if (Controller is not { } controller)
            {
                foreach (var (parameterId, value) in valueDictionary)
                {
                    if (TryGetParameterById(parameterId, out var parameter))
                    {
                        parameter.NormalizedValue = value;
                    }
                }
            }
            else
            {
                if (_threadId == currentThreadId)
                {
                    try
                    {
                        foreach (var (parameterId, value) in valueDictionary)
                        {
                            if (TryGetParameterById(parameterId, out var parameter))
                            {
                                if (parameter.NormalizedValue != value)
                                {
                                    controller.BeginEditParameter(parameter);
                                    parameter.NormalizedValue = value;
                                    controller.EndEditParameter();
                                }
                            }
                        }

                        return;
                    }
                    catch (Exception ex)
                    {
                        TsumikiLogger.WriteException(ex);
                    }
                }

                // 非UIスレッドから呼び出された場合と、 BeginEditParameter の呼び出しに失敗した場合にここにフォールバックする
                using (var stream = new MemoryStream(count * 8))
                {
                    using (var writer = new PortableBinaryWriter(stream, false))
                    {
                        var parameterCount = ParameterCount;
                        for (var i = 0; i < parameterCount; i++)
                        {
                            var parameter = GetParameterByIndex(i);
                            var parameterId = (ushort)parameter.Id.Value;
                            var currentValue = parameter.NormalizedValue;
                            if (valueDictionary.TryGetValue(parameterId, out var value))
                            {
                                writer.WriteFloat64(value);
                                // 通知用のパラメータリストから除去
                                if (currentValue == value)
                                {
                                    valueDictionary.Remove(parameterId);
                                }
                            }
                            else
                            {
                                writer.WriteFloat64(currentValue);
                            }
                        }
                    }

                    stream.Seek(0, SeekOrigin.Begin);

                    using var innerReader = new PortableBinaryReader(stream, false);
                    base.Load(innerReader, mode);
                }

                controller.OnLoaded(valueDictionary);
            }
        }
        catch (Exception ex)
        {
            TsumikiLogger.WriteException(ex);
        }
    }
}
