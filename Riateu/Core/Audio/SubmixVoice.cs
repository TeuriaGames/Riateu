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
