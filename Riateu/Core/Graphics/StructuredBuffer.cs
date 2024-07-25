namespace Riateu.Graphics;

public class StructuredBuffer<T> : RawBuffer
where T : unmanaged
{
    public unsafe StructuredBuffer(GraphicsDevice device, BufferUsageFlags usageFlags, uint sizeInBytes) 
        : base(device, usageFlags, (uint)sizeof(T) * sizeInBytes)
    {
    }
}
   