namespace Riateu.Graphics;

public record struct ShaderStorage(uint StorageTextureCount, uint StorageBufferCount) 
{
    public static ShaderStorage Empty => new ShaderStorage(0, 0);
}