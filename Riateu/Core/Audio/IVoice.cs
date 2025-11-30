namespace Riateu.Audios;

public interface IVoice
{
    abstract static SourceVoice Create(VoiceMaker player, AudioDevice device, Format format);
}
