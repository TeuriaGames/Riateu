using System;
using System.IO;
using System.Runtime.InteropServices;

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

    private bool loadFromRefresh;
    private bool disposedValue;

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

    private unsafe void Load(Stream stream) 
    {
		var length = stream.Length;
		var buffer = NativeMemory.Alloc((nuint) length);
		var span = new Span<byte>(buffer, (int) length);
		stream.ReadExactly(span);

        fixed (byte* ptr = span) 
        {
            var pixelData = RiateuNative.Riateu_LoadImage(ptr, span.Length, out var w, out var h, out var _);

			Width = w;
			Height = h;
            data = (IntPtr)pixelData;
        }

		NativeMemory.Free(buffer);

        if (data == IntPtr.Zero) 
        {
            throw new Exception("Failed to load an image.");
        }

        loadFromRefresh = true;
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

    public unsafe void CopyFrom(Image image, int x, int y) 
    {
        CopyFrom(image.Pixels, x, y, image.Width, image.Height);
    }

    public int WriteQOI(string path) 
    {
        unsafe 
        {
            return RiateuNative.Riateu_WriteQOI(path, (byte*)data, Width, Height);
        }
    }

    public int WritePNG(string path) 
    {
        unsafe 
        {
            return RiateuNative.Riateu_WritePNG(path, (byte*)data, Width, Height);
        }
    }

    public enum Format { PNG, QOI }

    protected virtual unsafe void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (loadFromRefresh) 
            {
                RiateuNative.Riateu_FreeImage((byte*)data);
            }
            
            else 
            {
                NativeMemory.Free((void*)data);
            }

            data = IntPtr.Zero;
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
