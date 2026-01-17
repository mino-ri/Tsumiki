using NPlug.Validator;
using Tsumiki;

var factory = TsumikiPlugin.GetFactory();
if (!AudioPluginValidator.Validate(factory.Export, Console.Out, Console.Error))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("検証に失敗しました。");
}
