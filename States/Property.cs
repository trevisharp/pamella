/* Author:  Leonardo Trevisan Silio
 * Date:    04/04/2023
 */
using System;

namespace Pamella.States;

public class Porperty<T> : State
{
    private T value;
    private State state;

    public Porperty(State state)
        => this.state = state;

    public override void Change()
    {
        this.state.Change();
    }

    public override int GetHashCode()
        => this.value?.GetHashCode() ?? 0;

    public override string ToString()
        => value?.ToString() ?? null;

    private void tryChange(object value)
    {
        if (this.value.Equals(value))
            return;

        if (value is T data)
        {
            this.value = data;
            this.state.Changed();
            return;
        }
        
        throw new Exception(); // TODO: Change Exception Type
    }
    
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(this, obj))
            return true;

        if (ReferenceEquals(obj, null))
            return false;

        return this.value.Equals(obj);
    }

    public static bool operator ==(Porperty<T> prop, T obj)
        => prop.value.Equals(obj);

    public static bool operator !=(Porperty<T> prop, T obj)
        => !(prop == obj);

    public static Porperty<T> operator +(Porperty<T> prop, T obj)
    {
        dynamic value = prop.value;
        dynamic dyn = obj;
        dynamic sum = value + dyn;
        
        prop.tryChange(sum);

        return prop;
    }

    public static Porperty<T> operator -(Porperty<T> prop, T obj)
    {
        dynamic value = prop.value;
        dynamic dyn = obj;
        dynamic sum = value - dyn;
        
        prop.tryChange(sum);

        return prop;
    }

    public static Porperty<T> operator *(Porperty<T> prop, T obj)
    {
        dynamic value = prop.value;
        dynamic dyn = obj;
        dynamic sum = value * dyn;
        
        prop.tryChange(sum);

        return prop;
    }

    public static Porperty<T> operator /(Porperty<T> prop, T obj)
    {
        dynamic value = prop.value;
        dynamic dyn = obj;
        dynamic sum = value / dyn;
        
        prop.tryChange(sum);

        return prop;
    }

    public static Porperty<T> operator |(Porperty<T> prop, T obj)
    {
        prop.tryChange(obj);
        return prop;
    }
}