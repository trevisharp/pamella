/* Author:  Leonardo Trevisan Silio
 * Date:    04/04/2023
 */
using System;

namespace Pamella.States;

public class Property<T> : State
{
    private T value;
    private State state;

    public Property(State state)
        => this.state = state;

    public override void Changed()
    {
        this.state.Changed();
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

    public static bool operator ==(Property<T> prop, T obj)
        => prop.value.Equals(obj);

    public static bool operator !=(Property<T> prop, T obj)
        => !(prop == obj);

    public static bool operator >(Property<T> prop, T obj)
    {
        dynamic value = prop.value;
        dynamic dyn = obj;
        return value > dyn;
    }

    public static bool operator <(Property<T> prop, T obj)
    {
        dynamic value = prop.value;
        dynamic dyn = obj;
        return value < dyn;
    }

    public static bool operator >=(Property<T> prop, T obj)
    {
        dynamic value = prop.value;
        dynamic dyn = obj;
        return value >= dyn;
    }

    public static bool operator <=(Property<T> prop, T obj)
    {
        dynamic value = prop.value;
        dynamic dyn = obj;
        return value <= dyn;
    }

    public static T operator +(Property<T> prop, T obj)
    {
        dynamic value = prop.value;
        dynamic dyn = obj;
        dynamic sum = value + dyn;
        return (T)sum;
    }

    public static T operator -(Property<T> prop, T obj)
    {
        dynamic value = prop.value;
        dynamic dyn = obj;
        dynamic sub = value - dyn;
        return (T)sub;
    }

    public static T operator *(Property<T> prop, T obj)
    {
        dynamic value = prop.value;
        dynamic dyn = obj;
        dynamic mul = value * dyn;
        return (T)mul;
    }

    public static T operator /(Property<T> prop, T obj)
    {
        dynamic value = prop.value;
        dynamic dyn = obj;
        dynamic div = value / dyn;
        return (T)div;
    }

    public static Property<T> operator |(Property<T> prop, T obj)
    {
        prop.tryChange(obj);
        return prop;
    }
}