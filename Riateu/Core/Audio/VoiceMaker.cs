using System;
using System.Collections.Generic;

namespace Riateu.Audios;

public class VoiceMaker
{
    private Dictionary<(Type, Format), Queue<SourceVoice>> voicePool = new();
    private List<SoundVoice> trackedSoundVoices = new List<SoundVoice>();
    private List<SoundVoice> removingSoundVoices = new List<SoundVoice>();
    private AudioDevice device;
    private object StateLock = new object();

    public VoiceMaker(AudioDevice device) 
    {
        this.device = device;
    }

    public void Update() 
    {
        foreach (var voice in trackedSoundVoices) 
        {
            voice.Update();
        }

        foreach (var voice in removingSoundVoices) 
        {
            trackedSoundVoices.Remove(voice);
            voice.Reset();
            Queue<SourceVoice> queue = voicePool[(voice.GetType(), voice.Format)];
            queue.Enqueue(voice);
        }

        removingSoundVoices.Clear();
    }

    public void Destroy(SoundVoice voice) 
    {
        lock (StateLock) 
        {
            removingSoundVoices.Add(voice);
        }
    }

    public SourceVoice MakeSourceVoice<T>(Format format) 
    where T : IVoice
    {
        lock (StateLock) 
        {
            CreateQueueIfNothing(typeof(T), format);

            Queue<SourceVoice> queue = voicePool[(typeof(T), format)];
            if (queue.Count == 0) 
            {
                queue.Enqueue(T.Create(this, device, format));
            }
            
            SourceVoice voice = queue.Dequeue();

            if (voice is SoundVoice soundVoice) 
            {
                trackedSoundVoices.Add(soundVoice);
            }

            return voice;
        }
    }

    internal void CreateQueueIfNothing(Type type, Format format) 
    {
        if (!voicePool.ContainsKey((type, format))) 
        {
            voicePool.Add((type, format), new Queue<SourceVoice>());
        }
    }
}
