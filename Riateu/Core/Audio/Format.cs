namespace Riateu.Audios;

public struct Format
{
    public FormatTag Tag;
    public ushort Channels;
    public uint SampleRate;
    public ushort BitsPerSample;

    internal FAudio.FAudioWaveFormatEx ToFAudioFormat()
    {
        var blockAlign = (ushort)((BitsPerSample / 8) * Channels);

        return new FAudio.FAudioWaveFormatEx
        {
            wFormatTag = (ushort)Tag,
            nChannels = Channels,
            nSamplesPerSec = SampleRate,
            wBitsPerSample = BitsPerSample,
            nBlockAlign = blockAlign,
            nAvgBytesPerSec = blockAlign * SampleRate
        };
    }
}
