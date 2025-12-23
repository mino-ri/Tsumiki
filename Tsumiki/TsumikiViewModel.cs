using System;
using Tsumiki.View;

namespace Tsumiki;

partial class TsumikiViewModel : IParameterGroup
{
    public void BeginGroupEdit() => controller?.BeginGroupEditParameters();

    public void EndGroupEdit() => controller?.EndGroupEditParameters();
}
