using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using RefreshCS;

namespace Riateu.Graphics;

// Original implementation is from: https://github.com/MoonsideGames/MoonWorks/blob/refresh2/src/Graphics/ResourceUploader.cs

/// <summary>
/// A convenience structure for creating resources and uploading data to them.
///
/// Note that Upload or UploadAndWait must be called after the Create methods for the data to actually be uploaded.
///
/// Note that this structure does not magically keep memory usage down -
/// you may want to stagger uploads over multiple submissions to minimize memory usage.
/// </summary>
public unsafe class ResourceUploader : GraphicsResource
{
	TransferBuffer BufferTransferBuffer;
	TransferBuffer TextureTransferBuffer;

	byte* bufferData;
	uint bufferDataOffset = 0;
	uint bufferDataSize = 1024;

	byte* textureData;
	uint textureDataOffset = 0;
	uint textureDataSize = 1024;

	List<(uint, BufferRegion, bool)> BufferUploads = new List<(uint, BufferRegion, bool)>();
	List<(uint, TextureRegion, bool)> TextureUploads = new List<(uint, TextureRegion, bool)>();

	public ResourceUploader(GraphicsDevice device) : base(device)
	{
		bufferData = (byte*) NativeMemory.Alloc(bufferDataSize);
		textureData = (byte*) NativeMemory.Alloc(textureDataSize);
	}

	// Buffers

	/// <summary>
	/// Creates a Buffer with data to be uploaded.
	/// </summary>
	public StructuredBuffer<T> CreateBuffer<T>(Span<T> data, BufferUsageFlags usageFlags) where T : unmanaged
	{
		var buffer = new StructuredBuffer<T>(Device, usageFlags, (uint)data.Length);

		SetBufferData(buffer, 0, data, false);

		return buffer;
	}

	/// <summary>
	/// Prepares upload of data into a Buffer.
	/// </summary>
	public void SetBufferData<T>(RawBuffer buffer, uint bufferOffsetInElements, Span<T> data, bool cycle) where T : unmanaged
	{
		uint elementSize = (uint) Marshal.SizeOf<T>();
		uint offsetInBytes = elementSize * bufferOffsetInElements;
		uint lengthInBytes = (uint) (elementSize * data.Length);

		uint resourceOffset;
		fixed (void* spanPtr = data)
		{
			resourceOffset = CopyBufferData(spanPtr, lengthInBytes);
		}

		var bufferRegion = new BufferRegion(buffer, offsetInBytes, lengthInBytes);
		BufferUploads.Add((resourceOffset, bufferRegion, cycle));
	}

	// Textures

	public Texture CreateTexture2D<T>(Span<T> pixelData, uint width, uint height) where T : unmanaged
	{
        Texture texture = new Texture(Device, width, height, TextureFormat.R8G8B8A8, TextureUsageFlags.Sampler);
		SetTextureData(texture, pixelData, false);
		return texture;
	}

	/// <summary>
	/// Creates a 2D Texture from compressed image data to be uploaded.
	/// </summary>
	public Texture CreateTexture2DFromCompressed(Span<byte> compressedImageData)
	{
        fixed (byte *ptr = compressedImageData) 
        {
            Refresh.Refresh_Image_Info(ptr, compressedImageData.Length, out int width, out int height, out _);
            Texture texture = new Texture(Device, (uint)width, (uint)height, TextureFormat.R8G8B8A8, TextureUsageFlags.Sampler);
            SetTextureDataFromCompressed(texture, compressedImageData);
            return texture;
        }
	}

	public void SetTextureDataFromCompressed(TextureRegion textureRegion, Span<byte> compressedImageData)
	{
        fixed (byte *ptr = compressedImageData) 
        {
            var pixelData = Refresh.Refresh_Image_Load(ptr, compressedImageData.Length, out _, out _, out int sizeInBytes);
            var pixelSpan = new Span<byte>((void*) pixelData, (int) sizeInBytes);

            SetTextureData(textureRegion, pixelSpan, false);

            Refresh.Refresh_Image_Free(pixelData);
        }
	}

	/// <summary>
	/// Creates a 2D Texture from a compressed image stream to be uploaded.
	/// </summary>
	public Texture CreateTexture2DFromCompressed(Stream compressedImageStream)
	{
		var length = (uint)compressedImageStream.Length;
		byte *buffer = (byte*)NativeMemory.Alloc(length);
		
		var span = new Span<byte>((void*)buffer, (int) length);
		compressedImageStream.ReadExactly(span);

		Texture texture = CreateTexture2DFromCompressed(span);

		NativeMemory.Free(buffer);

		return texture;
	}

	/// <summary>
	/// Creates a 2D Texture from a compressed image file to be uploaded.
	/// </summary>
	public Texture CreateTexture2DFromCompressed(string compressedImageFilePath)
	{
		using var fileStream = File.OpenRead(compressedImageFilePath);
		return CreateTexture2DFromCompressed(fileStream);
	}

	/// <summary>
	/// Prepares upload of pixel data into a TextureSlice.
	/// </summary>
	public void SetTextureData<T>(TextureRegion textureRegion, Span<T> data, bool cycle) where T : unmanaged
	{
		var elementSize = Marshal.SizeOf<T>();
		var dataLengthInBytes = (uint) (elementSize * data.Length);

		uint resourceOffset;
		fixed (T* dataPtr = data)
		{
			resourceOffset = CopyTextureData(dataPtr, dataLengthInBytes, Texture.BytesPerPixel(textureRegion.TextureSlice.Texture.Format));
		}

		TextureUploads.Add((resourceOffset, textureRegion, cycle));
	}

	// Upload

	/// <summary>
	/// Uploads all the data corresponding to the created resources.
	/// </summary>
	public void Upload()
	{
		CopyToTransferBuffer();

		var commandBuffer = Device.AcquireCommandBuffer();
		RecordUploadCommands(commandBuffer);
		Device.Submit(commandBuffer);
	}

	/// <summary>
	/// Uploads and then blocks until the upload is finished.
	/// This is useful for keeping memory usage down during threaded upload.
	/// </summary>
	public void UploadAndWait()
	{
		CopyToTransferBuffer();

		var commandBuffer = Device.AcquireCommandBuffer();
		RecordUploadCommands(commandBuffer);
		var fence = Device.SubmitAndAcquireFence(commandBuffer);
		Device.WaitForFence(fence);
		Device.ReleaseFence(fence);
	}

	// Helper methods

	private void CopyToTransferBuffer()
	{
		if (BufferUploads.Count > 0)
		{
			if (BufferTransferBuffer == null || BufferTransferBuffer.Size < bufferDataSize)
			{
				BufferTransferBuffer?.Dispose();
				BufferTransferBuffer = new TransferBuffer(Device, TransferBufferUsage.Upload, bufferDataSize);
			}

			var dataSpan = new Span<byte>(bufferData, (int) bufferDataSize);
			BufferTransferBuffer.SetTransferData(dataSpan, 0, true);
		}


		if (TextureUploads.Count > 0)
		{
			if (TextureTransferBuffer == null || TextureTransferBuffer.Size < textureDataSize)
			{
				TextureTransferBuffer?.Dispose();
				TextureTransferBuffer = new TransferBuffer(Device, TransferBufferUsage.Upload, textureDataSize);
			}

			var dataSpan = new Span<byte>(textureData, (int) textureDataSize);
			TextureTransferBuffer.SetTransferData(dataSpan, 0, true);
		}
	}

	private void RecordUploadCommands(CommandBuffer commandBuffer)
	{
		var copyPass = commandBuffer.BeginCopyPass();

		foreach (var (transferOffset, bufferRegion, option) in BufferUploads)
		{
			copyPass.UploadToBuffer(
				new TransferBufferLocation(BufferTransferBuffer, transferOffset),
				bufferRegion,
				option
			);
		}

		foreach (var (transferOffset, textureRegion, option) in TextureUploads)
		{
			copyPass.UploadToTexture(
				new TextureTransferInfo(TextureTransferBuffer, transferOffset),
				textureRegion,
				option
			);
		}

		commandBuffer.EndCopyPass(copyPass);

		BufferUploads.Clear();
		TextureUploads.Clear();
		bufferDataOffset = 0;
	}

	private uint CopyBufferData(void* ptr, uint lengthInBytes)
	{
		if (bufferDataOffset + lengthInBytes >= bufferDataSize)
		{
			bufferDataSize = bufferDataOffset + lengthInBytes;
			bufferData = (byte*) NativeMemory.Realloc(bufferData, bufferDataSize);
		}

		var resourceOffset = bufferDataOffset;

		NativeMemory.Copy(ptr, bufferData + bufferDataOffset, lengthInBytes);
		bufferDataOffset += lengthInBytes;

		return resourceOffset;
	}

	private uint CopyTextureData(void* ptr, uint lengthInBytes, uint alignment)
	{
		textureDataOffset = RoundToAlignment(textureDataOffset, alignment);

		if (textureDataOffset + lengthInBytes >= textureDataSize)
		{
			textureDataSize = textureDataOffset + lengthInBytes;
			textureData = (byte*) NativeMemory.Realloc(textureData, textureDataSize);
		}

		var resourceOffset = textureDataOffset;

		NativeMemory.Copy(ptr, textureData + textureDataOffset, lengthInBytes);
		textureDataOffset += lengthInBytes;

		return resourceOffset;
	}

	private uint RoundToAlignment(uint value, uint alignment)
	{
		return alignment * ((value + alignment - 1) / alignment);
	}

	// Dispose

	/// <summary>
	/// It is valid to immediately call Dispose after calling Upload.
	/// </summary>
	protected override void Dispose(bool disposing)
	{
		if (disposing) 
		{
			BufferTransferBuffer?.Dispose();
			TextureTransferBuffer?.Dispose();
		}
        NativeMemory.Free(bufferData);
	}

    protected override void HandleDispose(nint handle) {}
}