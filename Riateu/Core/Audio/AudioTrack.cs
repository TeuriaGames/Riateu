using System;
using System.Runtime.InteropServices;

namespace Riateu.Audios;

public abstract class AudioTrack : AudioResource
{
    public Format Format;
    public uint LengthInBytes;


    public AudioTrack(AudioDevice device, Format format, IntPtr buffer, uint lengthInBytes) : base(device)
    {
        Format = format;
        Handle = buffer;
        LengthInBytes = lengthInBytes;
    }


    protected override void Dispose(bool disposing) {}

    protected override unsafe void HandleDispose(nint handle)
    {
        NativeMemory.Free((void*)handle);
    }

    public abstract FAudio.FAudioBuffer ToFAudioBuffer(bool loop = false);
}
