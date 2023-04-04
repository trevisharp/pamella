/* Author:  Leonardo Trevisan Silio
 * Date:    04/04/2023
 */
using System;
using System.Reflection;
using System.Collections.Generic;

namespace Pamella.States;

public abstract class State
{
    internal List<Watcher> watchers = null;

    public virtual void Changed()
    {
        foreach (var watcher in this.watchers)
            watcher.OnWatchUpdate();
    }

    private State()
    {
        this.watchers = new List<Watcher>();
        init();
    }

    private void init()
    {
        var type = GetType();

        foreach (var prop in type.GetRuntimeProperties())
        {
            var propType = prop.PropertyType;
            object[] parameters = { this };
            var property = Activator.CreateInstance(
                propType, parameters
            );
            prop.SetValue(this, property);
        }
    }
}