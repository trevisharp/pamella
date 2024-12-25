/* Author:  Leonardo Trevisan Silio
 * Date:    31/01/2023
 */
using System;

namespace Pamella;

public abstract class AutoInvalidationMode(Func<View, float, bool> condition)
{
    DateTime last = DateTime.MinValue;
    public bool NeedInvalidate(View view)
    {
        var invalidate = condition(view, getFrameSecs());
        if (invalidate)
            last = DateTime.Now;
        return invalidate;
    }

    private float getFrameSecs()
    {
        var now = DateTime.Now;
        var frameTime = now - last;
        return (float)frameTime.TotalSeconds;
    }
}

public class NeverInvalidate() : AutoInvalidationMode((_, _) => false);

public class AlwaysInvalidate() : AutoInvalidationMode((_, _) => true);

public class TimeInvalidate(float time) : AutoInvalidationMode((_, t) => t > time);