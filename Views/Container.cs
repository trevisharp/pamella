/* Author:  Leonardo Trevisan Silio
 * Date:    24/05/2023
 */
using System.Collections;
using System.Collections.Generic;

namespace Pamella.Views;

public class Container : View, ICollection<View>
{
    public bool IsReadOnly => false;
    public int Count => this.subviews.Count;

    public void Add(View view)
        => AddSubView(view);

    public void Clear()
        => this.subviews.Clear();

    public bool Contains(View item)
        => this.subviews.Contains(item);

    public void CopyTo(View[] array, int arrayIndex)
        => this.subviews.CopyTo(array, arrayIndex);

    public bool Remove(View item)
        => this.subviews.Remove(item);

    public IEnumerator<View> GetEnumerator()
        => this.subviews.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}