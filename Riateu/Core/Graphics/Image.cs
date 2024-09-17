using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace Riateu.Graphics;

public class Image : IDisposable
{
    public int Width { get; private set; }
    public int Height { get; private set; }

    public unsafe Span<Color> Pixels 
    {
        get 
        {
            if (Width <= 0 || Height <= 0)
                return Span<Color>.Empty;
            return new Span<Color>((void*)data, Width * Height);
        }
    }

    public IntPtr Data => data;
    private IntPtr data;

    private bool disposedValue;

    internal Image() {}

    public unsafe Image(int width, int height) 
        :this(width, height, Color.Transparent) {}

    public unsafe Image(int width, int height, Color fill) 
        :this(new Color[width * height], width, height)
    {
        Color* pixels = (Color*)data.ToPointer();
        for (int i = 0, n = width * height; i < n; i++) 
        {
            pixels[i] = fill;
        }
    }

    public unsafe Image(Span<byte> color, int width, int height) 
    {
        Width = width;
        Height = height;
        byte* ptr = (byte*)NativeMemory.Alloc((uint)color.Length * 4);
        for (int i = 0; i < color.Length; i += 4) 
        {
            ptr[i] = color[i];
            ptr[i + 1] = color[i + 1];
            ptr[i + 2] = color[i + 2];
            ptr[i + 3] = color[i + 3];
        }
        data = (IntPtr)ptr;
    }

    public unsafe Image(Span<Color> color, int width, int height) 
    {
        Width = width;
        Height = height;
        Color* ptr = (Color*)NativeMemory.Alloc((uint)color.Length * 4);
        for (int i = 0; i < color.Length; i++) 
        {
            ptr[i] = color[i].RGBA;
        }
        data = (IntPtr)ptr;
    }

    public Image(string path) 
    {
        using FileStream fs = File.OpenRead(path);
        Load(fs);
    }

    public Image(Stream stream) 
    {
        Load(stream);
    }

    public static unsafe Image[] LoadGif(string path) 
    {
        using var fs = File.OpenRead(path);
        Image[] gif = LoadGif(fs);
        if (gif == null) 
        {
            throw new Exception($"Failed to load GIF file from path: '{path}'");
        }
        return gif;
    }

    public static unsafe Image[] LoadGif(Stream stream) 
    {
        IntPtr buffer = stream.AllocToPointer(out int len, out Span<byte> span);
        fixed (byte *ptr = span) 
        {
            IntPtr gif = Native.Riateu_LoadGif((byte*)ptr, len);
            if (gif == IntPtr.Zero) 
            {
                return null;
            }
            Native.Riateu_GetGifFrames(gif, out int f);
            Native.Riateu_GetGifSize(gif, out int w, out int h, out _);
            Image[] images = new Image[f];
            for (int i = 0; i < f; i++) 
            {
                IntPtr p = Native.Riateu_CopyGifFrames(gif, i);
                Image image = new Image();
                image.data = p;
                image.Width = w;
                image.Height = h;
                images[i] = image;
            }

            Native.Riateu_FreeGif(gif);
            NativeMemory.Free((void*)buffer);
            return images;
        }
    }

    private unsafe void Load(Stream stream) 
    {
		var length = stream.Length;
		var buffer = NativeMemory.Alloc((nuint) length);
		var span = new Span<byte>(buffer, (int) length);
		stream.ReadExactly(span);

        fixed (byte* ptr = span) 
        {
            var pixelData = Native.Riateu_LoadImage(ptr, span.Length, out var w, out var h, out var _);

			Width = w;
			Height = h;
            data = (IntPtr)pixelData;
        }

		NativeMemory.Free(buffer);

        if (data == IntPtr.Zero) 
        {
            throw new Exception("Failed to load an image.");
        }
    }

    public unsafe void CopyFrom(ReadOnlySpan<Color> pixels, int x, int y, int srcWidth, int srcHeight)
    {
        Rectangle destination = new Rectangle(x, y, srcWidth, srcHeight);

        Rectangle dst = new Rectangle(0, 0, Width, Height).Overlap(destination);
        if (dst.Width <= 0 || dst.Height <= 0) 
        {
            return;
        }

        Point pixel = new Point(dst.X - destination.X, dst.Y - destination.Y); 

        fixed (Color* pixPtr = pixels) 
        {
            Color* dataPtr = (Color*)data.ToPointer();
            int size = dst.Width;

            for (int yh = 0; yh < dst.Height; yh++) 
            {
                Color* srcPtr = pixPtr + ((pixel.Y + yh) * srcWidth + pixel.X);
                Color* destPtr = dataPtr + ((dst.Y + yh) * Width + dst.X);

                NativeMemory.Copy(srcPtr, destPtr, (nuint)(size * 4));
            }
        }
    }

    public unsafe void Premultiply() 
    {
        fixed (Color *ptr = Pixels) 
        {
            byte alpha;
            for (int i = 0; i < Width * Height; i++) 
            {
                Color col = ptr[i];

                alpha = col.A;
                ptr[i].R *= alpha;
                ptr[i].G *= alpha;
                ptr[i].B *= alpha;
            }
        }
    }

    public unsafe void CopyFrom(Image image, int x, int y) 
    {
        CopyFrom(image.Pixels, x, y, image.Width, image.Height);
    }

    public int WriteQOI(string path) 
    {
        unsafe 
        {
            return Native.Riateu_WriteQOI(path, (byte*)data, Width, Height);
        }
    }

    public int WritePNG(string path) 
    {
        unsafe 
        {
            return Native.Riateu_WritePNG(path, (byte*)data, Width, Height);
        }
    }

    public Texture UploadAsTexture(GraphicsDevice device) 
    {
        Texture texture = new Texture(device);
        using TransferBuffer transferBuffer = new TransferBuffer(device, TransferBufferUsage.Upload, (uint)(Width * Height * 4));
        transferBuffer.SetTransferData(Pixels, 0, false);
        CommandBuffer buffer = device.AcquireCommandBuffer();

        CopyPass pass = buffer.BeginCopyPass();
        pass.UploadToTexture(transferBuffer, texture, false);
        buffer.EndCopyPass(pass);
        device.Submit(buffer);
        return texture;
    }

    public enum Format { PNG, QOI }

    protected virtual unsafe void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            IntPtr lockedPtr = Interlocked.Exchange(ref data, IntPtr.Zero);
            if (lockedPtr != IntPtr.Zero) 
            {
                Native.Riateu_FreeImage(lockedPtr);

                data = IntPtr.Zero;
            }

            GC.SuppressFinalize(this);
            disposedValue = true;
        }
    }

    ~Image()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
