namespace Riateu.Audios;

public class StaticVoice : SourceVoice, IVoice
{
    public StaticVoice(VoiceMaker maker, AudioDevice device, in Format format) : base(maker, device, format) {}

    public static SourceVoice Create(VoiceMaker player, AudioDevice device, Format format)
    {
        return new StaticVoice(player, device, format);
    }

    public override void Update()
    {
    }
}