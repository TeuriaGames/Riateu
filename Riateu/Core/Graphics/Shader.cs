using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using SDL3;

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
            var entryPointLength = Encoding.UTF8.GetByteCount(entryPoint) + 1;
			var entryPointBuffer = NativeMemory.Alloc((nuint) entryPointLength);
            var buffer = new Span<byte>(entryPointBuffer, entryPointLength);
			var byteCount = Encoding.UTF8.GetBytes(entryPoint, buffer);
            buffer[byteCount] = 0;

            SDL.SDL_GPUShaderCreateInfo gpuShaderCreateInfo = new SDL.SDL_GPUShaderCreateInfo() 
            {
                code_size = (nuint)bytes.Length,
                code = (byte*)b,
                entrypoint = (byte*)entryPointBuffer,
                stage = (SDL.SDL_GPUShaderStage)info.ShaderStage,
                format = (SDL.SDL_GPUShaderFormat)info.ShaderFormat,
                num_samplers = info.SamplerCount,
                num_storage_buffers = info.StorageBufferCount,
                num_storage_textures = info.StorageTextureCount,
                num_uniform_buffers = info.UniformBufferCount
            };

            IntPtr shaderPtr;

            if (GraphicsDevice.Backend == "vulkan") 
            {
                shaderPtr = SDL.SDL_CreateGPUShader(
                    device.Handle,
                    gpuShaderCreateInfo
                );
            }
            else 
            {
                shaderPtr = Native.Riateu_CompileSPIRVGraphics(
                    device.Handle,
                    gpuShaderCreateInfo
                );
            }


            if (shaderPtr == IntPtr.Zero) 
            {
                throw new InvalidOperationException("Shader compilation failed!");
            }

            NativeMemory.Free(entryPointBuffer);

            return shaderPtr;
        }
    }

    protected override void Dispose(bool disposing)
    {
    }

    protected override void HandleDispose(nint handle)
    {
        SDL.SDL_ReleaseGPUShader(Device.Handle, handle);
    }
}
