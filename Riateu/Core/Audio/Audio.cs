namespace Riateu.Audios;

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