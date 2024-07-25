using System;
using System.IO;
using System.Runtime.InteropServices;
using RefreshCS;

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

	private static unsafe nint CreateFromStream(
		GraphicsDevice device,
		Stream stream,
		string entryPointName,
		in ComputePipelineCreateInfo computePipelineCreateInfo
	) {
		var bytecodeBuffer = (byte*) NativeMemory.Alloc((nuint) stream.Length);
		var bytecodeSpan = new Span<byte>(bytecodeBuffer, (int) stream.Length);
		stream.ReadExactly(bytecodeSpan);

		Refresh.ComputePipelineCreateInfo refreshPipelineCreateInfo = new Refresh.ComputePipelineCreateInfo 
        {
            Code = bytecodeBuffer,
            CodeSize = (nuint) stream.Length,
            EntryPointName = entryPointName,
            Format = (Refresh.ShaderFormat) computePipelineCreateInfo.ShaderFormat,
            ReadOnlyStorageTextureCount = computePipelineCreateInfo.ReadOnlyStorageTextureCount,
            ReadOnlyStorageBufferCount = computePipelineCreateInfo.ReadOnlyStorageBufferCount,
            ReadWriteStorageTextureCount = computePipelineCreateInfo.ReadWriteStorageTextureCount,
            ReadWriteStorageBufferCount = computePipelineCreateInfo.ReadWriteStorageBufferCount,
            UniformBufferCount = computePipelineCreateInfo.UniformBufferCount,
            ThreadCountX = computePipelineCreateInfo.ThreadCountX,
            ThreadCountY = computePipelineCreateInfo.ThreadCountY,
            ThreadCountZ = computePipelineCreateInfo.ThreadCountZ
        };

		var computePipelineHandle = Refresh.Refresh_CreateComputePipeline(
			device.Handle,
			refreshPipelineCreateInfo
		);

		if (computePipelineHandle == nint.Zero)
		{
			throw new Exception("Could not create compute pipeline!");
		}

		NativeMemory.Free(bytecodeBuffer);
		return computePipelineHandle;
	}

    protected override void Dispose(bool disposing)
    {
    }

    protected override void HandleDispose(nint handle)
    {
        Refresh.Refresh_ReleaseComputePipeline(Device.Handle, handle);
    }
}
