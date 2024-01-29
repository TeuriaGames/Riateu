using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using AsyncKey = System.Collections.Generic.KeyValuePair<System.Threading.SendOrPostCallback, object>;

namespace Riateu.Misc;

public class CoroutineContext : SynchronizationContext
{
    private IList<AsyncKey> continuations = new List<AsyncKey>();
    public bool IsRunning => continuations.Count > 0;

    public override void Post(SendOrPostCallback d, object state)
    {
        if (d == null) 
        {
            throw new ArgumentNullException(nameof(d));
        }

        continuations.Add(new AsyncKey(d, state));
    }

    public override SynchronizationContext CreateCopy()
    {
        return this;
    }

    public void Update() 
    {
        AsyncKey[] toProcess = continuations.ToArray();
        continuations.Clear();
        foreach (AsyncKey continuation in toProcess) 
        {
            var currContext = SynchronizationContext.Current;

            try 
            {
                SynchronizationContext.SetSynchronizationContext(this);
                continuation.Key(continuation.Value);
            }
            finally 
            {
                SynchronizationContext.SetSynchronizationContext(currContext);
            }
        }
    }
}