using System;

namespace Riateu.Audios;

public class SourceVoice : BaseVoice, IVoice
{
    public int AudioTypeID => IVoice.SourceVoice;
    public SoundState State
    {
        get 
        {
            if (BuffersQueued == 0) 
            {
                Stop();
            }
            return state;
        }
    }
    private SoundState state;

    private VoiceMaker maker;

    public Format Format { get; }
    protected bool Initiated;

    protected readonly object StateLock = new object();

    internal unsafe SourceVoice(VoiceMaker maker, AudioDevice device, in Format format) 
        : base(device, format.Channels, device.DeviceDetails.OutputFormat.Format.nChannels)
    {
        Format = format;
        FAudio.FAudioWaveFormatEx fAudioFormat = format.ToFAudioFormat();
        FAudio.FAudio_CreateSourceVoice(
            device.Handle, out IntPtr handle, 
            ref fAudioFormat, 
            FAudio.FAUDIO_VOICE_USEFILTER, FAudio.FAUDIO_DEFAULT_FREQ_RATIO, 
            IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        Handle = handle;

        SetOutputVoice(device.MasterVoice);

        this.maker = maker;
    }

    public static SourceVoice Create(VoiceMaker player, AudioDevice device, Format format) 
    {
        return new SourceVoice(player, device, format);
    }

    public void Update() 
    {
        lock (StateLock) 
        {
            if (Initiated && BuffersQueued == 0) 
            {
                Stop();
                maker.Destroy(this);
            }
        }
    }

    public void Play() 
    {
        lock (StateLock) 
        {
            Initiated = true;
            FAudio.FAudioSourceVoice_Start(Handle, 0, FAudio.FAUDIO_COMMIT_NOW);

            state = SoundState.Playing;
        }
    }

    public void Submit(AudioTrack buffer) 
    {
        lock (StateLock) 
        {
            FAudio.FAudioBuffer fAudioBuffer = buffer.ToFAudioBuffer();
            FAudio.FAudioSourceVoice_SubmitSourceBuffer(Handle, ref fAudioBuffer, IntPtr.Zero);
        }
    }

    public void Stop() 
    {
        lock (StateLock) 
        {
            FAudio.FAudioSourceVoice_Stop(Handle, 0, FAudio.FAUDIO_COMMIT_NOW);
            FAudio.FAudioSourceVoice_FlushSourceBuffers(Handle);

            state = SoundState.Stopped;
        }
    }

    public void Pause() 
    {
        lock (StateLock) 
        {
            FAudio.FAudioSourceVoice_Stop(Handle, 0, FAudio.FAUDIO_COMMIT_NOW);
            state = SoundState.Paused;
        }
    }

    public override void Reset()
    {
        Stop();
        Initiated = false;
        base.Reset();
    }
}
