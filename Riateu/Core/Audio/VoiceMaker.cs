using System.Collections.Generic;

namespace Riateu.Audios;

public class VoiceMaker
{
    private Dictionary<(int, Format), Queue<SourceVoice>> voicePool = new();
    private List<SourceVoice> trackedSourceVoices = new List<SourceVoice>();
    private List<SourceVoice> removingSourceVoices = new List<SourceVoice>();
    private AudioDevice device;

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
            Queue<SourceVoice> queue = voicePool[(voice.AudioTypeID, voice.Format)];
            queue.Enqueue(voice);
        }

        removingSourceVoices.Clear();
    }

    public void Destroy(SourceVoice voice) 
    {
        removingSourceVoices.Add(voice);
    }

    public SourceVoice MakeSourceVoice(Format format) 
    {
        CreateQueueIfNothing(0, format);

        Queue<SourceVoice> queue = voicePool[(0, format)];
        if (queue.Count == 0) 
        {
            queue.Enqueue(SourceVoice.Create(this, device, format));
        }
        
        SourceVoice voice = queue.Dequeue();
        trackedSourceVoices.Add(voice);
        return voice;
    }

    internal void CreateQueueIfNothing(int id, Format format) 
    {
        if (!voicePool.ContainsKey((id, format))) 
        {
            voicePool.Add((id, format), new Queue<SourceVoice>());
        }
    }
}
