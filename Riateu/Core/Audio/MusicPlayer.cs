namespace Riateu.Audios;

public class MusicPlayer 
{
    public AudioDevice Device { get; internal set; }

    public float Pitch 
    {
        get => streamVoice.Pitch;
        set => streamVoice.Pitch = value;
    }

    public float Volume
    {
        get => streamVoice.Volume;
        set => streamVoice.Volume = value;
    }

    public float Pan
    {
        get => streamVoice.Pan;
        set => streamVoice.Pan= value;
    }

    private StreamVoice streamVoice;
    private AudioStream currentStream;
    public MusicPlayer(AudioDevice device) 
    {
        Device = device;
    }

    public void Play(AudioStream stream, bool looping = false) 
    {
        if (currentStream != stream) 
        {
            Stop();
            streamVoice = (StreamVoice)Device.VoiceMaker.MakeSourceVoice<StreamVoice>(stream.Format);
            streamVoice.Load(stream);
            currentStream = stream;
        }
        streamVoice.Looping = looping;

        streamVoice.Play();
    }

    public void Pause() 
    {
        streamVoice?.Pause();
    }

    public void Stop() 
    {
        streamVoice?.Stop();
    }
}