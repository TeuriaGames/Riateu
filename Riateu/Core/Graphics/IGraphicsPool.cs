using System;

namespace Riateu.Graphics;

internal interface IGraphicsPool 
{
    void Obtain(GraphicsDevice device);
    void Reset();
}

internal interface IPassPool 
{
    void Obtain(IntPtr handle);
    void Reset();
}