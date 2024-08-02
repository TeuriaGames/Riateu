using System;
using System.Runtime.InteropServices;

namespace Riateu.Audios;

public class AudioTrackOGG : AudioTrack
{
    private uint loopStart;
    private uint loopEnd;
    public AudioTrackOGG(AudioDevice device, Format format, nint buffer, uint lengthInBytes) : base(device, format, buffer, lengthInBytes)
    {
    }

    public static unsafe AudioTrack CreateOGG(AudioDevice device, string path) 
    {
        IntPtr ptr = FAudio.stb_vorbis_open_filename(path, out int err, IntPtr.Zero);

        if (err != 0) 
        {
            throw new Exception("Error loading a file, probably file not found!");
        }

        FAudio.stb_vorbis_info info = FAudio.stb_vorbis_get_info(ptr);
        float lengthInFloats = FAudio.stb_vorbis_stream_length_in_samples(ptr) * info.channels;
        float lengthInBytes = lengthInFloats * sizeof(float);
        void* buffer = (byte*)NativeMemory.Alloc((nuint)lengthInBytes);

        FAudio.stb_vorbis_get_samples_float_interleaved(ptr, info.channels, (IntPtr)buffer, (int)lengthInFloats);

        FAudio.stb_vorbis_close(ptr);

        Format format = new Format() 
        {
            Tag = FormatTag.IEEE_FLOAT,
            BitsPerSample = 32,
            Channels = (ushort)info.channels,
            SampleRate = info.sample_rate
        };

        AudioTrackOGG ogg = new AudioTrackOGG(device, format, (IntPtr)buffer, (uint)lengthInBytes);
        ogg.FindLoop();

        return ogg;
    }

    private unsafe void FindLoop()
    {
        loopStart = SearchComment("LOOPSTART=");
        loopEnd = SearchComment("LOOPEND=");

        uint SearchComment(string field) 
        {
            uint value = 0;

            FAudio.stb_vorbis_comment commment = FAudio.stb_vorbis_get_comment(Handle);
            for (int i = 0; i < commment.comment_list_length; i++) 
            {
                nint pointer = Marshal.ReadIntPtr(commment.comment_list, i * sizeof(IntPtr));
                string s = Marshal.PtrToStringAnsi(pointer);
                if (s.StartsWith(field)) 
                {
                    UInt32.TryParse(s.Substring(field.Length), out value);
                }
            }

            return value;
        }
        if (loopEnd == 0) 
        {
            loopEnd = LengthInBytes;
        }
    }


    public override FAudio.FAudioBuffer ToFAudioBuffer(bool loop = false) 
    {
        return new FAudio.FAudioBuffer() 
        {
            Flags = FAudio.FAUDIO_END_OF_STREAM,
            pContext = IntPtr.Zero,
            pAudioData = Handle,
            AudioBytes = LengthInBytes,
            PlayBegin = 0,
            PlayLength = 0,
            LoopBegin = 0,
            LoopLength = 0, // TODO fix this
            LoopCount = loop ? FAudio.FAUDIO_LOOP_INFINITE : 0
        };
    }
}
