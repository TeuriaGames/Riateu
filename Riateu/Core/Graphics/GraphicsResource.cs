using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Riateu.Graphics;

public abstract class GraphicsResource : IDisposable
{
    public IntPtr Handle 
    { 
        get => handle;
        internal set => handle = value;
    }
    public bool IsDisposed { get; internal set; }
    public GraphicsDevice Device { get; internal set; }

    private IntPtr handle;

    private GCHandle selfReference;

    public GraphicsResource(GraphicsDevice device) 
    {
        Device = device;
        selfReference = GCHandle.Alloc(this, GCHandleType.Weak);
        Device.AddReference(selfReference);
    }

    protected abstract void Dispose(bool disposing);

    protected abstract void HandleDispose(IntPtr handle);

    private void InternalDispose(bool disposing) 
    {
        if (disposing) 
        {
            Device.RemoveReference(selfReference);
            selfReference.Free();
        }

        IntPtr lockedHandlePtr = Interlocked.Exchange(ref handle, IntPtr.Zero);

        if (lockedHandlePtr != IntPtr.Zero) 
        {
            HandleDispose(lockedHandlePtr);
        }

        Dispose(disposing);
    }

    ~GraphicsResource()
    {
        if (IsDisposed) 
        {
            return;
        }
#if DEBUG
        Console.WriteLine($"Leaked Resources: {this.GetType().Name}. Make sure to dispose this properly.");
#endif
        InternalDispose(disposing: false);
        IsDisposed = true;
    }

    public void Dispose()
    {
        if (IsDisposed) 
        {
            return;
        }
        InternalDispose(disposing: true);
        GC.SuppressFinalize(this);
        IsDisposed = true;
    }
}
