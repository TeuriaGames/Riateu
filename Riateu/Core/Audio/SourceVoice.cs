using System;

namespace Riateu.Audios;

public abstract class SourceVoice : BaseVoice
{
    public bool Looping { get; set; }
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

    public VoiceMaker Maker => maker;
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

    public abstract void Update();
    

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
        Submit(buffer.ToFAudioBuffer(Looping));
    }

    protected void Submit(in FAudio.FAudioBuffer fbuffer) 
    {
        lock (StateLock) 
        {
            FAudio.FAudioBuffer buffer = fbuffer;
            FAudio.FAudioSourceVoice_SubmitSourceBuffer(Handle, ref buffer, IntPtr.Zero);
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
