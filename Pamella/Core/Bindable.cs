/* Author:  Leonardo Trevisan Silio
 * Date:    18/11/2026
 */
using System;
using System.Linq.Expressions;
using System.Collections.Generic;

using Blindness.Bind;

namespace Pamella;

/// <summary>
/// Represents a constructor argument: absent, a fixed value or a binding.
/// </summary>
public readonly struct Bindable<T>
{
    public T Value { get; }
    public bool HasValue { get; }
    public Expression<Func<T>> Source { get; }
    public bool IsBound => Source is not null;

    Bindable(T value)
    {
        Value = value;
        HasValue = true;
        Source = null;
    }

    public Bindable(Expression<Func<T>> source)
    {
        Value = default;
        HasValue = false;
        Source = source;
    }

    public static implicit operator Bindable<T>(T value) => new(value);
}

/// <summary>
/// Represents a readonly box computed from the boxes read by a function.
/// </summary>
sealed class ComputedBox<T> : IBox<T>, INotifyBox, IDisposable
{
    readonly Func<T> function;
    HashSet<INotifyBox> reads = [];
    T value;
    bool stale = true;

    public ComputedBox(Func<T> function)
    {
        this.function = function;
        Refresh();
    }

    public event Action Changed;

    public bool IsReadonly => true;

    public T Open()
    {
        BoxTracker.Read(this);
        if (stale)
            Refresh();
        return value;
    }

    public void Place(T value)
        => throw new InvalidOperationException("A computed binding cannot receive values.");

    public void Dispose()
    {
        foreach (var box in reads)
            box.Changed -= OnChanged;
        reads = [];
    }

    void Refresh()
    {
        HashSet<INotifyBox> current = [];
        value = BoxTracker.Track(function, current);
        stale = false;

        foreach (var box in reads)
            if (!current.Contains(box))
                box.Changed -= OnChanged;

        foreach (var box in current)
            if (!reads.Contains(box))
                box.Changed += OnChanged;

        reads = current;
    }

    void OnChanged()
    {
        stale = true;
        Changed?.Invoke();
    }
}
