using Tsumiki.View;

namespace Tsumiki.View.WinFormsTest;

public partial class TsumikiViewTest : Form
{
    private IDisposable? _control;

    public TsumikiViewTest()
    {
        InitializeComponent();
    }

    private void TsumikiViewTest_Shown(object sender, EventArgs e)
    {
        _control = ViewInitializer.AttackInParentWindow(Handle);
    }

    private void TsumikiViewTest_FormClosing(object sender, FormClosingEventArgs e)
    {
        _control?.Dispose();
    }
}
