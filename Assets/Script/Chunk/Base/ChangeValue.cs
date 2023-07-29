using System;
public class ChangeValue<T> where T : IComparable
{
    private T _value;

    private T _maxValue;

    public T MaxValue
    {
        get { return _maxValue; }
    }

    private T _minValue;
    public T MinValue
    {
        get { return _minValue; }
    }

    private Action<T, T> _changeAction;
    private Action _changeActionSimple;

    public T Value
    {
        get
        {
            return _value;
        }
    }

    public ChangeValue(T value, T min, T max)
    {
        _minValue = min;
        _maxValue = max;
        _value = value;
    }

    public void Set(T newValue)
    {
        T oldValue = _value;
        //Limit
        _value = Utility.Clamp<T>(newValue, _minValue, _maxValue);

        _changeAction?.Invoke(oldValue, newValue);
        _changeActionSimple?.Invoke();
    }

    public void SetMaxValue(T maxValue)
    {
        T currentValue = _value;

        if (_maxValue.CompareTo(maxValue) < 0)
        {
            _maxValue = maxValue;
        }
        else if (_maxValue.CompareTo(maxValue) > 0)
        {
            _maxValue = maxValue;
            Set(currentValue);
        }
    }

    public void BindChangeAction(Action<T, T> changeAction, ref T currentValue)
    {
        _changeAction += changeAction;
        currentValue = _value;
    }

    public void UnBindChangeAction(Action<T, T> changeAction)
    {
        _changeAction -= changeAction;
    }

    public void BindChangeAction(Action<T, T> changeAction)
    {
        _changeAction += changeAction;
    }

    public void UnBindChangeAction(Action changeAction)
    {
        _changeActionSimple -= changeAction;
    }

    public void BindChangeAction(Action changeAction)
    {
        _changeActionSimple += changeAction;
    }


    public void Dispose()
    {
        _changeAction = null;
    }
}