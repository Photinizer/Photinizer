namespace Photinizer.Common;

internal class Cached<T>(Func<T> getFunc)
{
    protected T? _value;

    public T Get(bool force = false)
        => force || _value is null ? _value = getFunc() : _value;
}