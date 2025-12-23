using Tsumiki.Core;
using Tsumiki.Metadata;

namespace Tsumiki.View;

[VstModel("Tsumiki", typeof(ITsumikiModel))]
public partial interface ITsumikiViewModel : IParameterGroup;

public interface IViewParameter
{
    public double NormalizedValue { get; set; }
    public double DefaultNormalizedValue { get; }
    public int Id { get; }
    public void BeginEdit();
    public void EndEdit();
}

public interface IViewParameter<T> : IViewParameter
{
    public T Value { get; set; }
    public T DefaultValue { get; }
}

public interface IRangeViewParameter<T> : IViewParameter<T>
{
    public T MinValue { get; }
    public T MaxValue { get; }
    public int StepCount { get; }
}

public interface IParameterGroup
{
    public void BeginGroupEdit();
    public void EndGroupEdit();
}
