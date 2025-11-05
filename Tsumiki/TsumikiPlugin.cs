using System.Runtime.CompilerServices;
using NPlug;

namespace Tsumiki;
public static class TsumikiPlugin
{
    public static AudioPluginFactory GetFactory()
    {
        var factory = new AudioPluginFactory(new("Minori", "https://github.com/mino-ri/Tsumiki", "hojo.origami@gmail.com"));
        factory.RegisterPlugin(new AudioProcessorClassInfo(SampleMidiProcessor.ClassId, "Tsumiki", AudioProcessorCategory.Instrument), () => new SampleMidiProcessor());
        factory.RegisterPlugin(new AudioControllerClassInfo(SampleMidiController.ClassId, "Tsumiki Controller"), () => new SampleMidiController());
        return factory;
    }

    [ModuleInitializer]
    internal static void ExportThisPlugin()
    {
        AudioPluginFactoryExporter.Instance = GetFactory();
    }
}
