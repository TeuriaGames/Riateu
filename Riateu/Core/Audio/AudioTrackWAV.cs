using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Riateu.Audios;

public class AudioTrackWAV : AudioTrack
{
    public AudioTrackWAV(AudioDevice device, Format format, nint buffer, uint lengthInBytes) : base(device, format, buffer, lengthInBytes)
    {
    }

    public static unsafe AudioTrack CreateWAV(AudioDevice device, string path) 
    {
			// mostly borrowed from https://github.com/FNA-XNA/FNA/blob/b71b4a35ae59970ff0070dea6f8620856d8d4fec/src/Audio/SoundEffect.cs#L385

            using var stream = File.OpenRead(path);
			using var reader = new BinaryReader(stream);

			string signature = new string(reader.ReadChars(4));
			if (signature != "RIFF")
			{
				throw new NotSupportedException("Specified stream is not a wave file.");
			}

			reader.ReadUInt32(); // Riff Chunk Size

			string wformat = new string(reader.ReadChars(4));
			if (wformat != "WAVE")
			{
				throw new NotSupportedException("Specified stream is not a wave file.");
			}

			string format_signature = new string(reader.ReadChars(4));
			while (format_signature != "fmt ")
			{
				reader.ReadBytes(reader.ReadInt32());
				format_signature = new string(reader.ReadChars(4));
			}

			int format_chunk_size = reader.ReadInt32();

			ushort wFormatTag = reader.ReadUInt16();
			ushort nChannels = reader.ReadUInt16();
			uint nSamplesPerSec = reader.ReadUInt32();
			uint nAvgBytesPerSec = reader.ReadUInt32();
			ushort nBlockAlign = reader.ReadUInt16();
			ushort wBitsPerSample = reader.ReadUInt16();

			if (format_chunk_size > 16)
			{
				reader.ReadBytes(format_chunk_size - 16);
			}

			string dataSignature = new string(reader.ReadChars(4));
			while (dataSignature.ToLowerInvariant() != "data")
			{
				reader.ReadBytes(reader.ReadInt32());
				dataSignature = new string(reader.ReadChars(4));
			}
			if (dataSignature != "data")
			{
				throw new NotSupportedException("Specified wave file is not supported.");
			}

			int waveDataLength = reader.ReadInt32();
			var waveDataBuffer = NativeMemory.Alloc((nuint) waveDataLength);
			var waveDataSpan = new Span<byte>(waveDataBuffer, waveDataLength);
			stream.ReadExactly(waveDataSpan);

			var format = new Format
			{
				Tag = (FormatTag) wFormatTag,
				BitsPerSample = wBitsPerSample,
				Channels = nChannels,
				SampleRate = nSamplesPerSec
			};

			return new AudioTrackWAV(device, format, (nint) waveDataBuffer, (uint) waveDataLength);
    }

    public override FAudio.FAudioBuffer ToFAudioBuffer(bool loop = false)
    {
        return new FAudio.FAudioBuffer() 
        {
            Flags = FAudio.FAUDIO_END_OF_STREAM,
            pContext = IntPtr.Zero,
            pAudioData = Handle,
            AudioBytes = LengthInBytes,
            PlayBegin = 0,
            PlayLength = 0,
            LoopBegin = 0,
            LoopLength = 0,
            LoopCount = loop ? FAudio.FAUDIO_LOOP_INFINITE : 0
        };
    }
}