using System;
using System.Collections.Generic;

namespace Riateu;

public class Picker<T>
{
    private List<Option> options;

    public float EvaluatedWeight { get; private set; }
    public bool CanPick => EvaluatedWeight > 0;
    public int Count => options.Count;

    public Picker() 
    {
        options = new List<Option>();
    }

    public Picker(T firstOption, float weight) : this()
    {
        AddOption(firstOption, weight);
    }

    public void AddOption(T option, float weight) 
    {
        float w = Math.Max(weight, 0);
        EvaluatedWeight += weight;
        options.Add(new Option(option, w));
    }

    public void AddOption(Span<T> options, float weight) 
    {
        foreach (var option in options) 
        {
            AddOption(option, weight);
        }
    }

    public void AddOption(ReadOnlySpan<T> options, float weight) 
    {
        foreach (var option in options) 
        {
            AddOption(option, weight);
        }
    }

    public void AddOption(List<T> options, float weight) 
    {
        foreach (var option in options) 
        {
            AddOption(option, weight);
        }
    }

    public void AddOption(T[] options, float weight) 
    {
        foreach (var option in options) 
        {
            AddOption(option, weight);
        }
    }

    public T ForcePick() 
    {
        if (options.Count == 1)
            return options[0].Value;

        var w = 0f;
        var roll = Random.Shared.NextDouble() * EvaluatedWeight;
        var optionCount = options.Count;

        for (int i = 0; i < optionCount; i++)
        {
            var option = options[i];
            w += option.Weight;
            if (roll < w)
                return option.Value;
        }
        return options[optionCount].Value;
    }

    public T Pick()
    {
        if (options.Count == 1) 
        {
            return options[0].Value;
        }

        if (!CanPick)
            return default;
        
        var w = 0f;
        var roll = Random.Shared.NextDouble() * EvaluatedWeight;
        var optionCount = options.Count;

        for (int i = 0; i < optionCount; i++)
        {
            var option = options[i];
            w += option.Weight;
            if (roll < w)
                return option.Value;
        }
        return options[optionCount].Value;
    }

    public void Clear() 
    {
        EvaluatedWeight = 0;
        options.Clear();
    }

    private struct Option 
    {
        public T Value;
        public float Weight;

        public Option(T value, float weight) 
        {
            Value = value;
            Weight = weight;
        }
    }
}