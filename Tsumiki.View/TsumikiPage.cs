namespace Tsumiki.View;

public class TsumikiPage(ITsumikiViewModel data) : Control<ITsumikiViewModel>(data, new RectF(0f, 0f, 1f, 1f))
{
    public override void RenderCore(IDrawingContext context) { }
}
