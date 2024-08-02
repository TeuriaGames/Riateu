using System;

namespace Riateu.Audios;

public class SubmixVoice : BaseVoice, IVoice
{
    public int AudioTypeID => IVoice.SubmixVoice;
    public SubmixVoice(AudioDevice device, uint srcChannelCount, uint sampleRate, uint processingStage) 
        : base(device, srcChannelCount, device.DeviceDetails.OutputFormat.Format.nChannels)
    {
        FAudio.FAudio_CreateSubmixVoice(
            device.Handle,
            out IntPtr handle,
            srcChannelCount,
            sampleRate,
            FAudio.FAUDIO_VOICE_USEFILTER,
            processingStage,
            IntPtr.Zero,
            IntPtr.Zero
        );

        Handle = handle;

        SetOutputVoice(device.MasterVoice);
    }

    public SubmixVoice(AudioDevice device) 
        : base(device, device.DeviceDetails.OutputFormat.Format.nChannels, device.DeviceDetails.OutputFormat.Format.nChannels)
    {
        FAudio.FAudio_CreateSubmixVoice(
            device.Handle,
            out IntPtr handle,
            device.DeviceDetails.OutputFormat.Format.nChannels,
            device.DeviceDetails.OutputFormat.Format.nSamplesPerSec,
            FAudio.FAUDIO_VOICE_USEFILTER,
            int.MaxValue,
            IntPtr.Zero,
            IntPtr.Zero
        );
        Handle = handle;
    }

    public static SubmixVoice CreateMasterVoice(AudioDevice device) 
    {
        return new SubmixVoice(device);
    }
}

public static class Audio 
{
    public static AudioDevice Device { get; private set; }

    public static float MasterVolume 
    {
        get => Device.MasterVoice.Volume;
        set => Device.MasterVoice.Volume = value;
    }

    public static float MasterPitch
    {
        get => Device.MasterVoice.Pitch;
        set => Device.MasterVoice.Pitch = value;
    }

    public static float MasterPan
    {
        get => Device.MasterVoice.Pan;
        set => Device.MasterVoice.Pan = value;
    }

    internal static void Init(AudioDevice device) 
    {
        Device = device;
    }

    public static void PlaySound(AudioTrack track) 
    {
        SourceVoice voice = Device.VoiceMaker.MakeSourceVoice(track.Format);
        voice.Submit(track);
        voice.Play();
    }

    public static void PlaySound(AudioTrack track, float volume = 1f, float pitch = 0f, float pan = 0f) 
    {
        SourceVoice voice = Device.VoiceMaker.MakeSourceVoice(track.Format);
        voice.Volume = volume;
        voice.Pitch = pitch;
        voice.Pan = pan;
        voice.Submit(track);
        voice.Play();
    }
}