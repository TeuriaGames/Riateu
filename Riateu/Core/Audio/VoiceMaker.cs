using System;
using System.Collections.Generic;

namespace Riateu.Audios;

public class VoiceMaker
{
    private Dictionary<(Type, Format), Queue<SourceVoice>> voicePool = new();
    private List<SourceVoice> trackedSourceVoices = new List<SourceVoice>();
    private List<SourceVoice> removingSourceVoices = new List<SourceVoice>();
    private AudioDevice device;
    private object StateLock = new object();

    public VoiceMaker(AudioDevice device) 
    {
        this.device = device;
    }

    public void Update() 
    {
        foreach (var voice in trackedSourceVoices) 
        {
            voice.Update();
        }

        foreach (var voice in removingSourceVoices) 
        {
            trackedSourceVoices.Remove(voice);
            voice.Reset();
            Queue<SourceVoice> queue = voicePool[(voice.GetType(), voice.Format)];
            queue.Enqueue(voice);
        }

        removingSourceVoices.Clear();
    }

    public void Destroy(SourceVoice voice) 
    {
        removingSourceVoices.Add(voice);
    }

    public SourceVoice MakeSourceVoice<T>(Format format) 
    where T : IVoice
    {
        CreateQueueIfNothing(typeof(T), format);

        Queue<SourceVoice> queue = voicePool[(typeof(T), format)];
        if (queue.Count == 0) 
        {
            queue.Enqueue(T.Create(this, device, format));
        }
        
        SourceVoice voice = queue.Dequeue();
        lock (StateLock) 
        {
            trackedSourceVoices.Add(voice);
        }
        return voice;
    }

    internal void CreateQueueIfNothing(Type type, Format format) 
    {
        if (!voicePool.ContainsKey((type, format))) 
        {
            voicePool.Add((type, format), new Queue<SourceVoice>());
        }
    }
}
