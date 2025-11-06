using System.Runtime.CompilerServices;
using NPlug;

namespace Tsumiki;
public static class TsumikiPlugin
{
    public static AudioPluginFactory GetFactory()
    {
        var factory = new AudioPluginFactory(new("Minori", "https://github.com/mino-ri/Tsumiki", "hojo.origami@gmail.com"));
        factory.RegisterPlugin(new AudioProcessorClassInfo(TsumikiProcessor.ClassId, "Tsumiki", AudioProcessorCategory.Instrument), () => new TsumikiProcessor());
        factory.RegisterPlugin(new AudioControllerClassInfo(TsumikiController.ClassId, "Tsumiki Controller"), () => new TsumikiController());
        return factory;
    }

    [ModuleInitializer]
    internal static void ExportThisPlugin()
    {
        AudioPluginFactoryExporter.Instance = GetFactory();
    }
}
