using System;
using System.Runtime.InteropServices;

namespace Riateu.Audios;

public class StreamVoice : SourceVoice, IVoice
{
    public const int BufferCount = 3;
    private IntPtr[] buffers;
    private int next;
    public AudioStream AudioStream { get; private set; }

    public uint BufferSize { get; private set; }

    internal StreamVoice(VoiceMaker maker, AudioDevice device, in Format format) : base(maker, device, format)
    {
        buffers = new IntPtr[BufferCount];
    }

    public unsafe void Load(AudioStream audioStream) 
    {
        lock (StateLock) 
        {
            if (AudioStream != null) 
            {
                AudioStream.Unload();
            }
            audioStream.Load();
            audioStream.Loaded = true;
            AudioStream = audioStream;

            BufferSize = audioStream.BufferSize;

            for (int i = 0; i < BufferCount; i++) 
            {
                if (buffers[i] != IntPtr.Zero) 
                {
                    NativeMemory.Free((void*)buffers[i]);
                }

                buffers[i] = (IntPtr)NativeMemory.Alloc(BufferSize);
            }

            CreateBuffer();
        }
    }

    public override void Update() 
    {
        lock (StateLock) 
        {
            if (State != SoundState.Playing) 
            {
                return;
            }

            CreateBuffer();
        }
    }

    private void CreateBuffer() 
    {
        int buffersNeeded = BufferCount - (int)BuffersQueued;
        for (int i = 0; i < buffersNeeded; i++) 
        {
            next = (next + 1) % BufferCount;
            IntPtr buffer = buffers[next];

            int length = AudioStream.CreateBuffer(buffer, (int)BufferSize, out bool hasEnded);

            if (length > 0) 
            {
                FAudio.FAudioBuffer fBuffer = new FAudio.FAudioBuffer() 
                {
                    AudioBytes = (uint)length,
                    pAudioData = buffer,
                    PlayLength = ((uint)length / Format.Channels / (uint)(Format.BitsPerSample / 8))
                };

                Submit(fBuffer);
            }

            if (hasEnded) 
            {
                if (Looping) 
                {
                    AudioStream.Seek(AudioStream.LoopStart);
                }
            }
        }
    }


    public void Unload() 
    {
        lock (StateLock) 
        {
            if (AudioStream != null) 
            {
                Stop();
                AudioStream.Unload();
                AudioStream = null;
            }
        }
    }

    public override void Reset()
    {
        Unload();
        base.Reset();
    }

    public static SourceVoice Create(VoiceMaker player, AudioDevice device, Format format)
    {
        return new StreamVoice(player, device, format);
    }

    protected override unsafe void Dispose(bool disposing)
    {
        lock (StateLock) 
        {
            Stop();

            for (int i = 0; i < BufferCount; i++) 
            {
                if (buffers[i] != IntPtr.Zero) 
                {
                    NativeMemory.Free((void*)buffers[i]);
                }
            }
        }
        base.Dispose(disposing);
    }
}