/* Author:  Leonardo Trevisan Silio
 * Date:    05/02/2023
 */
using Blindness.Concurrency;

namespace Pamella.Customizations;

public class ScreenAsyncElement(IScreenImplementation impl) : IAsyncElement
{
    public void Finish()
        => impl.Close();

    public void Start()
        => impl.Open();

    public void Wait() { }
}