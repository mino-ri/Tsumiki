namespace Tsumiki.View;

public partial class TsumikiPage
{
    public static TsumikiPage Create(ITsumikiViewModel data)
    {
        return new TsumikiPage(data) { };
    }
}
