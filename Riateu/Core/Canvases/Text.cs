using System;
using MoonWorks;
using MoonWorks.Graphics;
using MoonWorks.Graphics.Font;
using MoonWorks.Math.Float;
using Riateu.Graphics;

namespace Riateu;

public abstract class Text : IDisposable
{
    internal TextBatch Batch;
    public Texture Texture { get; protected set; }
    protected int pixelSize;
    protected string text;
    private bool IsDisposed { get; set; }

    public Rectangle Bounds { get; protected set; }

    public abstract void Draw(IBatch batch, Vector2 position);

    protected virtual void Dispose(bool disposing)
    {
        if (!IsDisposed)
        {
            if (disposing)
            {
                Batch.Dispose();
            }

            IsDisposed = true;
        }
    }

    ~Text()
    {
#if DEBUG
        Logger.LogWarn($"The type {this.GetType()} has not been disposed properly.");
#endif
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
