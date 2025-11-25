using Tsumiki.Core;
using Tsumiki.Metadata;

namespace Tsumiki.View;

[VstModel("Tsumiki", typeof(ITsumikiModel))]
public partial interface ITsumikiViewModel;

public interface IViewParameter<T>
{
    public T Value { get; set; }
    public T DefaultValue { get; }
}

public interface IRangeViewParameter<T> : IViewParameter<T>
    where T : IComparable<T>
{
    public T MinValue { get; }
    public T MaxValue { get; }
    public int StepCount { get; }
}
