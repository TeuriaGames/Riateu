using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using SDL3;

namespace Riateu.Graphics;

public class ComputePipeline : GraphicsResource
{
	public uint ReadOnlyStorageTextureCount { get; }
	public uint ReadOnlyStorageBufferCount { get; }
	public uint ReadWriteStorageTextureCount { get; }
	public uint ReadWriteStorageBufferCount { get; }
	public uint UniformBufferCount { get; }

	public ComputePipeline(
		GraphicsDevice device,
		string filePath,
		string entryPointName,
		in ComputePipelineCreateInfo computePipelineCreateInfo
	) : base(device)
	{
		using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
		Handle = CreateFromStream(device, stream, entryPointName, computePipelineCreateInfo);

		ReadOnlyStorageTextureCount = computePipelineCreateInfo.ReadOnlyStorageTextureCount;
		ReadOnlyStorageBufferCount = computePipelineCreateInfo.ReadOnlyStorageBufferCount;
		ReadWriteStorageTextureCount = computePipelineCreateInfo.ReadWriteStorageTextureCount;
		ReadWriteStorageBufferCount = computePipelineCreateInfo.ReadWriteStorageBufferCount;
		UniformBufferCount = computePipelineCreateInfo.UniformBufferCount;
	}

	public ComputePipeline(
		GraphicsDevice device,
		Stream stream,
		string entryPointName,
		in ComputePipelineCreateInfo computePipelineCreateInfo
	) : base(device)
	{
		Handle = CreateFromStream(device, stream, entryPointName, computePipelineCreateInfo);

		ReadOnlyStorageTextureCount = computePipelineCreateInfo.ReadOnlyStorageTextureCount;
		ReadOnlyStorageBufferCount = computePipelineCreateInfo.ReadOnlyStorageBufferCount;
		ReadWriteStorageTextureCount = computePipelineCreateInfo.ReadWriteStorageTextureCount;
		ReadWriteStorageBufferCount = computePipelineCreateInfo.ReadWriteStorageBufferCount;
		UniformBufferCount = computePipelineCreateInfo.UniformBufferCount;
	}

	public unsafe ComputePipeline(
		GraphicsDevice device,
		Span<byte> byteCode,
		string entryPointName,
		in ComputePipelineCreateInfo computePipelineCreateInfo
	) : base(device)
	{
		fixed (byte *ptr = byteCode)
		{
			Handle = CreateFromBytecode(device, ptr, byteCode.Length, entryPointName, computePipelineCreateInfo);
		}

		ReadOnlyStorageTextureCount = computePipelineCreateInfo.ReadOnlyStorageTextureCount;
		ReadOnlyStorageBufferCount = computePipelineCreateInfo.ReadOnlyStorageBufferCount;
		ReadWriteStorageTextureCount = computePipelineCreateInfo.ReadWriteStorageTextureCount;
		ReadWriteStorageBufferCount = computePipelineCreateInfo.ReadWriteStorageBufferCount;
		UniformBufferCount = computePipelineCreateInfo.UniformBufferCount;
	}

	private static unsafe nint CreateFromStream(
		GraphicsDevice device,
		Stream stream,
		string entryPointName,
		in ComputePipelineCreateInfo computePipelineCreateInfo
	) {
		var bytecodeBuffer = (byte*) NativeMemory.Alloc((nuint) stream.Length);
		try 
		{
			var bytecodeSpan = new Span<byte>(bytecodeBuffer, (int) stream.Length);
			stream.ReadExactly(bytecodeSpan);
			return CreateFromBytecode(device, bytecodeBuffer, stream.Length, entryPointName, computePipelineCreateInfo);
		}
		finally 
		{
			NativeMemory.Free(bytecodeBuffer);
		}
	}

	private static unsafe nint CreateFromBytecode(
		GraphicsDevice device,
		byte *bytecodeBuffer,
		long length,
		string entryPointName,
		in ComputePipelineCreateInfo computePipelineCreateInfo
	)
	{
		var entryPointLength = Encoding.UTF8.GetByteCount(entryPointName) + 1;
		var entryPointBuffer = (byte*)NativeMemory.Alloc((nuint) entryPointLength);
		var buffer = new Span<byte>(entryPointBuffer, entryPointLength);
		var byteCount = Encoding.UTF8.GetBytes(entryPointName, buffer);
		buffer[byteCount] = 0;

		SDL.SDL_GPUComputePipelineCreateInfo gpuPipelineCreateInfo = new SDL.SDL_GPUComputePipelineCreateInfo 
		{
            code = bytecodeBuffer,
            code_size = (nuint) length,
            entrypoint = (byte*)entryPointBuffer,
            format = (SDL.SDL_GPUShaderFormat) computePipelineCreateInfo.ShaderFormat,
            num_readonly_storage_textures = computePipelineCreateInfo.ReadOnlyStorageTextureCount,
            num_readonly_storage_buffers = computePipelineCreateInfo.ReadOnlyStorageBufferCount,
            num_readwrite_storage_textures = computePipelineCreateInfo.ReadWriteStorageTextureCount,
            num_readwrite_storage_buffers = computePipelineCreateInfo.ReadWriteStorageBufferCount,
            num_uniform_buffers = computePipelineCreateInfo.UniformBufferCount,
			num_samplers = computePipelineCreateInfo.SamplersCount,
            threadcount_x = computePipelineCreateInfo.ThreadCountX,
            threadcount_y = computePipelineCreateInfo.ThreadCountY,
            threadcount_z = computePipelineCreateInfo.ThreadCountZ
		};

		IntPtr computePipelineHandle = SDL.SDL_CreateGPUComputePipeline(
			device.Handle,
			gpuPipelineCreateInfo
		);
		

		if (computePipelineHandle == nint.Zero)
		{
			throw new Exception("Could not create compute pipeline!");
		}

		NativeMemory.Free(entryPointBuffer);
		return computePipelineHandle;
	}

    protected override void Dispose(bool disposing)
    {
    }

    protected override void HandleDispose(nint handle)
    {
		SDL.SDL_ReleaseGPUComputePipeline(Device.Handle, handle);
    }
}
