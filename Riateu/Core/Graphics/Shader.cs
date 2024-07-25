using System;
using System.IO;
using System.Runtime.InteropServices;
using RefreshCS;

namespace Riateu.Graphics;

public class Shader : GraphicsResource
{
    public uint SamplerCount { get; }
    public uint StorageTextureCount { get; }
    public uint StorageBufferCount { get; }
    public uint UniformBufferCount { get; }

    public unsafe Shader(GraphicsDevice device, string filePath, string entryPoint, in ShaderCreateInfo info) : base(device)
    {
        using Stream stream = File.OpenRead(filePath);

        Handle = Create(device, stream, entryPoint, info);

        SamplerCount = info.SamplerCount;
        StorageBufferCount = info.StorageBufferCount;
        StorageTextureCount = info.StorageTextureCount;
        UniformBufferCount = info.UniformBufferCount;
    }

    public unsafe Shader(GraphicsDevice device, Stream stream, string entryPoint, in ShaderCreateInfo info) : base(device)
    {
        Handle = Create(device, stream, entryPoint, info);

        SamplerCount = info.SamplerCount;
        StorageBufferCount = info.StorageBufferCount;
        StorageTextureCount = info.StorageTextureCount;
        UniformBufferCount = info.UniformBufferCount;
    }

    private static unsafe IntPtr Create(GraphicsDevice device, Stream stream, string entryPoint, in ShaderCreateInfo info) 
    {
        void *byteCodeBuffer = NativeMemory.Alloc((nuint)stream.Length);
        Span<byte> byteCodeSpan = new Span<byte>(byteCodeBuffer, (int)stream.Length);
        stream.ReadExactly(byteCodeSpan);

        Refresh.ShaderCreateInfo refreshShaderCreateInfo = new Refresh.ShaderCreateInfo 
        {
            CodeSize = (nuint)stream.Length,
            Code = (byte*)byteCodeBuffer,
            EntryPointName = entryPoint,
            Stage = (Refresh.ShaderStage)info.ShaderStage,
            Format = (Refresh.ShaderFormat)info.ShaderFormat,
            SamplerCount = info.SamplerCount,
            StorageBufferCount = info.StorageBufferCount,
            StorageTextureCount = info.StorageTextureCount,
            UniformBufferCount = info.UniformBufferCount
        };

        IntPtr shaderPtr = Refresh.Refresh_CreateShader(device.Handle, refreshShaderCreateInfo);

        if (shaderPtr == IntPtr.Zero) 
        {
            throw new InvalidOperationException("Shader compilation failed!");
        }

        NativeMemory.Free(byteCodeBuffer);
        return shaderPtr;
    }

    protected override void Dispose(bool disposing)
    {
    }

    protected override void HandleDispose(nint handle)
    {
        Refresh.Refresh_ReleaseShader(Device.Handle, handle);
    }
}
