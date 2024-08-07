using System;

namespace Riateu.Audios;

public abstract class AudioStream : AudioResource
{
    public bool Loaded { get; internal set; }
    public Format Format { get; internal set; }
    public uint LoopStart { get; internal set; }
    public uint LoopEnd { get; internal set; }
    public abstract uint BufferSize { get; }

    protected AudioStream(AudioDevice device) : base(device)
    {
    }

    public void Update() 
    {

    }

    public abstract void Load();
    public abstract void Unload();
    public abstract unsafe int CreateBuffer(IntPtr buffer, int sample, out bool hasEnded);
    public abstract void Seek(uint sampleFrame);
}
