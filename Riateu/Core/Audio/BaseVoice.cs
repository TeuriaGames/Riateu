using System;
using System.Runtime.InteropServices;

namespace Riateu.Audios;

public abstract class BaseVoice : AudioResource
{
    public uint BuffersQueued 
    {
        get 
        {
            FAudio.FAudioSourceVoice_GetState(Handle, out FAudio.FAudioVoiceState voiceState, FAudio.FAUDIO_VOICE_NOSAMPLESPLAYED);
            return voiceState.BuffersQueued;
        }
    }
    private SubmixVoice outputVoice;
    public SubmixVoice OutputVoice => outputVoice;

    private float volume = 1;

    public float Volume 
    {
        get => volume;
        set 
        {
            value = MathF.Max(0, value);
            if (volume != value) 
            {
                volume = value;
                FAudio.FAudioVoice_SetVolume(Handle, volume, FAudio.FAUDIO_COMMIT_NOW);
            }
        }
    }

    private float pan;

    public float Pan 
    {
        get => pan;
        set 
        {
            value = MathUtils.Clamp(value, -1f, 1f);
            if (pan != value) 
            {
                pan = value;

                UpdatePan();
            }
        }
    }

    private float dopplerFactor;
    public float DopplerFactor 
    {
        get => dopplerFactor;
        set 
        {
            if (dopplerFactor != value) 
            {
                dopplerFactor = value;
                UpdateFactor();
            }
        }
    }

    private float pitch;
    public float Pitch 
    {
        get => pitch;
        set 
        {
            value = MathUtils.Clamp(value, -1f, 1f);
            if (pitch != value) 
            {
                pitch = value;
                UpdateFactor();
            }
        }
    }

    public uint SourceChannelCount { get; }
    public uint DestinationChannelCount { get; }

    private unsafe byte* pMatrixCoefficients;

    protected unsafe BaseVoice(AudioDevice device, uint srcChannelCount, uint destChannelCount) : base(device)
    {
        SourceChannelCount = srcChannelCount;
        DestinationChannelCount = destChannelCount;
        nuint memSize = 4 * srcChannelCount * destChannelCount;
        pMatrixCoefficients = (byte*)NativeMemory.AllocZeroed(memSize);
        SetPanMatrixCoefficients();
    }

    public virtual void Reset() 
    {
        Volume = 1;
        Pan = 0;
        Pitch = 0;
        SetOutputVoice(Device.MasterVoice);
    }

    protected override void Dispose(bool disposing) {}

    protected override unsafe void HandleDispose(nint handle)
    {
        NativeMemory.Free(pMatrixCoefficients);
        FAudio.FAudioVoice_DestroyVoice(handle);
    }
    
    internal void UpdatePan() 
    {
        SetPanMatrixCoefficients();
        unsafe {
            FAudio.FAudioVoice_SetOutputMatrix(Handle, outputVoice.Handle, SourceChannelCount, DestinationChannelCount, (nint)pMatrixCoefficients, 0);
        }
    }

    internal void UpdateFactor() 
    {
        float doppler;
        float dopplerScale = Device.DopplerScale;
        if (dopplerScale == 0.0f) 
        {
            doppler = 1.0f;
        }
        else 
        {
            doppler = dopplerFactor * dopplerScale;
        }

        FAudio.FAudioSourceVoice_SetFrequencyRatio(Handle, (float)Math.Pow(2.0, pitch) * doppler, FAudio.FAUDIO_COMMIT_NOW);
    }

    public unsafe void SetOutputVoice(SubmixVoice voice) 
    {
        outputVoice = voice;
        FAudio.FAudioSendDescriptor *sendDesc = stackalloc FAudio.FAudioSendDescriptor[1];
        sendDesc[0].Flags = 0;
        sendDesc[0].pOutputVoice = voice.Handle;

        FAudio.FAudioVoiceSends sends = new FAudio.FAudioVoiceSends();
        sends.SendCount = 1;
        sends.pSends = (IntPtr)sendDesc;

        FAudio.FAudioVoice_SetOutputVoices(Handle, ref sends);
    }

    // Taken from https://github.com/FNA-XNA/FNA/blob/master/src/Audio/SoundEffectInstance.cs
    private unsafe void SetPanMatrixCoefficients()
    {
        /* Two major things to notice:
            * 1. The spec assumes any speaker count >= 2 has Front Left/Right.
            * 2. Stereo panning is WAY more complicated than you think.
            *    The main thing is that hard panning does NOT eliminate an
            *    entire channel; the two channels are blended on each side.
            * -flibit
            */
        float* outputMatrix = (float*) pMatrixCoefficients;
        if (SourceChannelCount == 1)
        {
            if (DestinationChannelCount == 1)
            {
                outputMatrix[0] = 1.0f;
            }
            else
            {
                outputMatrix[0] = (pan > 0.0f) ? (1.0f - pan) : 1.0f;
                outputMatrix[1] = (pan < 0.0f) ? (1.0f + pan) : 1.0f;
            }
        }
        else
        {
            if (DestinationChannelCount == 1)
            {
                outputMatrix[0] = 1.0f;
                outputMatrix[1] = 1.0f;
            }
            else
            {
                if (pan <= 0.0f)
                {
                    // Left speaker blends left/right channels
                    outputMatrix[0] = 0.5f * pan + 1.0f;
                    outputMatrix[1] = 0.5f * -pan;
                    // Right speaker gets less of the right channel
                    outputMatrix[2] = 0.0f;
                    outputMatrix[3] = pan + 1.0f;
                }
                else
                {
                    // Left speaker gets less of the left channel
                    outputMatrix[0] = -pan + 1.0f;
                    outputMatrix[1] = 0.0f;
                    // Right speaker blends right/left channels
                    outputMatrix[2] = 0.5f * pan;
                    outputMatrix[3] = 0.5f * -pan + 1.0f;
                }
            }
        }
    }
}
