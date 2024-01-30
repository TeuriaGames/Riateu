using System;
using MoonWorks;
using MoonWorks.Graphics;
using MoonWorks.Graphics.Font;
using MoonWorks.Math.Float;
using Riateu.Graphics;

namespace Riateu;

/// <summary>
/// The base class for the Text object.
/// </summary>
public abstract class Text : IDisposable
{
    internal TextBatch Batch;
    /// <summary>
    /// The texture of a text after it rendered.
    /// </summary>
    public Texture Texture { get; protected set; }
    private bool IsDisposed { get; set; }

    /// <summary>
    /// A bounds of the rendered text.
    /// </summary>
    public Rectangle Bounds { get; protected set; }

    /// <summary>
    /// A method that should be called in the draw loop.
    /// </summary>
    /// <param name="batch">A batch system</param>
    /// <param name="position">A position of the text</param>
    public abstract void Draw(IBatch batch, Vector2 position);

    /// <summary>
    /// Dispose resources.
    /// </summary>
    /// <param name="disposing">Dispose the unmanaged resources</param>
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

    ///
    ~Text()
    {
#if DEBUG
        Logger.LogWarn($"The type {this.GetType()} has not been disposed properly.");
#endif
        Dispose(disposing: false);
    }

    /// <summary>
    /// Dispose all resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
