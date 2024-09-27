using System;
using System.IO;
using System.Runtime.InteropServices;

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
        void *byteCodeBuffer = NativeMemory.Alloc((nuint)stream.Length);
        Span<byte> byteCodeSpan = new Span<byte>(byteCodeBuffer, (int)stream.Length);
        stream.ReadExactly(byteCodeSpan);

        Handle = Create(device, byteCodeSpan, entryPoint, info);

        NativeMemory.Free(byteCodeBuffer);

        SamplerCount = info.SamplerCount;
        StorageBufferCount = info.StorageBufferCount;
        StorageTextureCount = info.StorageTextureCount;
        UniformBufferCount = info.UniformBufferCount;
    }

    public unsafe Shader(GraphicsDevice device, Stream stream, string entryPoint, in ShaderCreateInfo info) : base(device)
    {
        void *byteCodeBuffer = NativeMemory.Alloc((nuint)stream.Length);
        Span<byte> byteCodeSpan = new Span<byte>(byteCodeBuffer, (int)stream.Length);
        stream.ReadExactly(byteCodeSpan);

        Handle = Create(device, byteCodeSpan, entryPoint, info);

        NativeMemory.Free(byteCodeBuffer);

        SamplerCount = info.SamplerCount;
        StorageBufferCount = info.StorageBufferCount;
        StorageTextureCount = info.StorageTextureCount;
        UniformBufferCount = info.UniformBufferCount;
    }

    public unsafe Shader(GraphicsDevice device, Span<byte> bytes, string entryPoint, in ShaderCreateInfo info) : base(device)
    {
        Handle = Create(device, bytes, entryPoint, info);

        SamplerCount = info.SamplerCount;
        StorageBufferCount = info.StorageBufferCount;
        StorageTextureCount = info.StorageTextureCount;
        UniformBufferCount = info.UniformBufferCount;
    }

    private static unsafe IntPtr Create(GraphicsDevice device, in Span<byte> bytes, string entryPoint, in ShaderCreateInfo info) 
    {
        fixed (byte* b = bytes) 
        {
            Refresh.ShaderCreateInfo refreshShaderCreateInfo = new Refresh.ShaderCreateInfo 
            {
                CodeSize = (nuint)bytes.Length,
                Code = (byte*)b,
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

            return shaderPtr;
        }
    }

    protected override void Dispose(bool disposing)
    {
    }

    protected override void HandleDispose(nint handle)
    {
        Refresh.Refresh_ReleaseShader(Device.Handle, handle);
    }
}
