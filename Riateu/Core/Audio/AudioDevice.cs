using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace Riateu.Audios;

public class AudioDevice : IDisposable
{
    public IntPtr Handle { get; internal set; }
    private HashSet<GCHandle> resources = new HashSet<GCHandle>();
    internal FAudio.FAudioDeviceDetails DeviceDetails;
    private Thread updateThread;
    private AutoResetEvent resetEvent;
    private TimeSpan updateInterval = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / 200);
    private Stopwatch tickWatch = new Stopwatch();
    private long previousTickTime;

    private IntPtr masteringVoicePtr;
    private SubmixVoice masterVoice;
    public SubmixVoice MasterVoice => masterVoice;

    public float DopplerScale = 1f;

    public bool IsRunning { get; private set; }

    internal readonly Lock ThreadLock = new Lock();

    public VoiceMaker VoiceMaker { get; }

    internal unsafe AudioDevice() 
    {
        FAudio.FAudioCreate(out IntPtr handle, 0, FAudio.FAUDIO_DEFAULT_PROCESSOR);
        Handle = handle;
        FAudio.FAudio_GetDeviceCount(Handle, out uint deviceCount);

        if (deviceCount == 0) 
        {
            Logger.Warn("No Audio devices found in this machine. Won't enabling AudioDevice");
            FAudio.FAudio_Release(Handle);
            Handle = IntPtr.Zero;
            return;
        }

        uint i = 0;

        FAudio.FAudioDeviceDetails deviceDetails;

        for (i = 0; i < deviceCount; i++) 
        {
            FAudio.FAudio_GetDeviceDetails(Handle, i, out deviceDetails);
            if ((deviceDetails.Role & FAudio.FAudioDeviceRole.FAudioDefaultGameDevice) == FAudio.FAudioDeviceRole.FAudioDefaultGameDevice) 
            {
                this.DeviceDetails = deviceDetails;
                break;
            }
        }

        if (i == deviceCount) 
        {
            i = 0;
            FAudio.FAudio_GetDeviceDetails(Handle, 0, out deviceDetails);
            this.DeviceDetails = deviceDetails;
        }

        uint result = FAudio.FAudio_CreateMasteringVoice(Handle, out masteringVoicePtr, FAudio.FAUDIO_DEFAULT_CHANNELS, FAudio.FAUDIO_DEFAULT_SAMPLERATE, 0, i, IntPtr.Zero);

        if (result != 0) 
        {
            Logger.Warn("Audio device failed to create a mastering voice. Disabling Audio device.");
            FAudio.FAudio_Release(Handle);
            Handle = IntPtr.Zero;
            return;
        }

        masterVoice = SubmixVoice.CreateMasterVoice(this);


        resetEvent = new AutoResetEvent(true);
        VoiceMaker = new VoiceMaker(this);

        updateThread = new Thread(Update);
        updateThread.IsBackground = true;
        updateThread.Start();

        IsRunning = true;

        tickWatch.Start();
        previousTickTime = 0;

        Logger.Info("Audio Device Created successfully!");
    }

    private void Update() 
    {
        while (IsRunning) 
        {
            lock (ThreadLock) 
            {
                try 
                {
                    long delta = tickWatch.Elapsed.Ticks - previousTickTime;
                    previousTickTime = tickWatch.Elapsed.Ticks;
                    float elapsedSeconds = (float) delta / System.TimeSpan.TicksPerSecond;

                    VoiceMaker.Update();
                }
                catch (Exception e) 
                {
                    Logger.Error("[ERROR] " + e.ToString());
                }
            }
            resetEvent.WaitOne(updateInterval);
        }
    }

    private bool IsDisposed;

    protected virtual void Dispose(bool disposing)
    {
        if (!IsDisposed)
        {
            if (disposing)
            {
                foreach (var handle in resources) 
                {
                    if (handle.Target is AudioResource res) 
                    {
                        res.Dispose();
                    }
                }

                resources.Clear();
            }

            if (masteringVoicePtr != IntPtr.Zero) 
            {
                FAudio.FAudioVoice_DestroyVoice(masteringVoicePtr);
            }

            if (Handle != IntPtr.Zero) 
            {
                FAudio.FAudio_Release(Handle);
            }


            IsDisposed = true;
        }
    }

    internal void Reset() 
    {
        resetEvent.Set();
    }

    public void AddReference(GCHandle handle) 
    {
        lock (resources) 
        {
            resources.Add(handle);
        }
    }

    public void RemoveReference(GCHandle handle) 
    {
        lock (resources) 
        {
            resources.Remove(handle);
        }
    }

    ~AudioDevice()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
