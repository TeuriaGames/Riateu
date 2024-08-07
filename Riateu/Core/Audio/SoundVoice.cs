namespace Riateu.Audios;

public class SoundVoice : SourceVoice, IVoice
{
    internal SoundVoice(VoiceMaker maker, AudioDevice device, in Format format) : base(maker, device, format)
    {
    }

    static SourceVoice IVoice.Create(VoiceMaker player, AudioDevice device, Format format)
    {
        return new SoundVoice(player, device, format);
    }

    public override void Update()
    {
        lock (StateLock) 
        {
            if (Initiated && BuffersQueued == 0) 
            {
                Stop();
                Maker.Destroy(this);
            }
        }
    }
}
