using NPlug.Validator;

namespace Tsumiki.Test;

public static class IntegrationTest
{
    [Fact]
    public static void Factory()
    {
        var factory = TsumikiPlugin.GetFactory();
        var outWriter = new StringWriter();
        var errorWriter = new StringWriter();
        if (!AudioPluginValidator.Validate(factory.Export, outWriter, errorWriter))
        {
            throw new Exception(outWriter.ToString() + "\n\n" + errorWriter.ToString());
        }
    }
}
