using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Riateu.Audios;

public class AudioStreamOGG : AudioStream
{
    private IntPtr fileDataPtr;
    private IntPtr actualHandle;
    private string filePath;


    public override uint BufferSize => 32768;

    public AudioStreamOGG(AudioDevice device, string filePath) : base(device)
    {
        this.filePath = filePath;
        IntPtr ptr = FAudio.stb_vorbis_open_filename(filePath, out int error, IntPtr.Zero);

        if (error != 0) 
        {
            throw new Exception("Cannot load the ogg file.");
        }

        FAudio.stb_vorbis_info info = FAudio.stb_vorbis_get_info(ptr);

        Format = new Format() 
        {
            Tag = FormatTag.IEEE_FLOAT,
            BitsPerSample = 32,
            Channels = (ushort)info.channels,
            SampleRate = info.sample_rate
        };

        var sampleCount = FAudio.stb_vorbis_stream_length_in_samples(ptr);
        FindLoop(ptr);
        if (LoopEnd == 0) 
        {
            LoopEnd = sampleCount;
        }

        FAudio.stb_vorbis_close(ptr);
    }

    private void FindLoop(IntPtr ptr)
    {
        LoopStart = Decode("LOOPSTART=");
        LoopEnd = Decode("LOOPEND=");

        uint Decode(string field) 
        {
            uint value = 0;

            FAudio.stb_vorbis_comment commment = FAudio.stb_vorbis_get_comment(ptr);
            for (int i = 0; i < commment.comment_list_length; i++) 
            {
                nint pointer = Marshal.ReadIntPtr(commment.comment_list, i * Marshal.SizeOf<IntPtr>());
                string s = Marshal.PtrToStringAnsi(pointer);
                if (s.StartsWith(field)) 
                {
                    UInt32.TryParse(s.Substring(field.Length), out value);
                }
            }

            return value;
        }
    }

    public override unsafe void Load()
    {
        if (Loaded)
        {
            return;
        }
        using var fs = File.OpenRead(filePath);
        fileDataPtr = fs.AllocToPointer(out int length);

        actualHandle = FAudio.stb_vorbis_open_memory(fileDataPtr, length, out int error, IntPtr.Zero);

        if (error != 0) 
        {
            throw new Exception($"Cannot read the audio file from memory. '{filePath}'");
        }
    }

    public override unsafe int CreateBuffer(IntPtr buffer, int sample, out bool hasEnded)
    {
        var lengthInFloats = sample / sizeof(float);

        var sampleCount = FAudio.stb_vorbis_get_samples_float_interleaved(actualHandle, Format.Channels, buffer, lengthInFloats) * Format.Channels;
        hasEnded = sampleCount < lengthInFloats;
        return sampleCount * sizeof(float);
    }

    public override void Seek(uint sampleFrame)
    {
        FAudio.stb_vorbis_seek(actualHandle, sampleFrame);
    }

    public override unsafe void Unload()
    {
        if (!Loaded) 
        {
            return;
        }

        FAudio.stb_vorbis_close(actualHandle);
        NativeMemory.Free((void*)fileDataPtr);

        actualHandle = IntPtr.Zero;
        fileDataPtr = IntPtr.Zero;
    }

    protected override unsafe void Dispose(bool disposing)
    {
        Unload();
    }

    protected override unsafe void HandleDispose(nint handle)
    {
    }
}
