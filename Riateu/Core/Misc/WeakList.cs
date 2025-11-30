using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Riateu;

// https://github.com/prime31/Nez/blob/master/Nez.Portable/Utils/Collections/FastList.cs
public class WeakList<T>
{
    private T[] buffer;
    public int Count;

    public T this[int idx] 
    {
        get => buffer[idx];
        set 
        {
#if DEBUG
            var objCheck = buffer[idx];
            if (objCheck == null) 
            {
                throw new Exception("You cannot replace an object that doesn't exist yet or null. If you meant to add an element, use Add(T)");
            }
#endif
            buffer[idx] = value;
        }
    } 

    public WeakList(int size) 
    {
        buffer = new T[size];
    }

    public WeakList() : this(5) {}

    public void Add(T item) 
    {
        if (Count == buffer.Length) 
        {
            Array.Resize(ref buffer, Math.Max(buffer.Length << 1, 10));
        }
        buffer[Count++] = item;
    }

    public void Remove(T item) 
    {
        var comp = EqualityComparer<T>.Default;
        for (int i = 0; i < Count; i++) 
        {
            if (comp.Equals(buffer[i], item)) 
            {
                RemoveAt(i);
                return;
            }
        }
    }

    public void RemoveAt(int index) 
    {
        if (index >= Count) 
        {
            throw new Exception($"Index {index} out of range!");
        }
        Count--;
        if (index < Count)
            Array.Copy(buffer, index + 1, buffer, index, Count - index);
        buffer[Count] = default!;
    }

    public bool Contains(T item) 
    {
        var comp = EqualityComparer<T>.Default;
        for (int i = 0; i < Count; ++i) 
        {
            if (comp.Equals(buffer[i], item))
                return true;
        }
        return false;
    }

    public int IndexOf(T item) 
    {
        return Array.IndexOf(buffer, item);
    }

    public void EnsureCapacity(int addition = 1) 
    {
        if (Count + addition >= buffer.Length)
            Array.Resize(ref buffer, Math.Max(buffer.Length << 1, Count + addition));
    }

    public void AddRange(IEnumerable<T> array) 
    {
        foreach (var item in array)
            Add(item);
    }

    public void Sort(IComparer comparer) 
    {
        Array.Sort(buffer, 0, Count, comparer);
    }

    public void Sort(IComparer<T> comparer) 
    {
        Array.Sort<T>(buffer, 0, Count, comparer);
    }

    public void Sort(Comparison<T> comparison) 
    {
        if (Count > 0) 
        {
            var comparer = new WeakComparer<T>(comparison);
            Array.Sort<T>(buffer, 0, Count, comparer);
        }

    }

    public void Clear() 
    {
        Array.Clear(buffer, 0, Count);
        Count = 0;
    }

    public T[] ToArray() 
    {
        T[] newBuff = new T[Count];
        Array.Copy(buffer, newBuff, Count);
        return newBuff;
    }

    public List<T> ToList() 
    {
        var list = new List<T>(buffer);
        list.RemoveAll(t => t is null);
        return list;
    }

    public Span<T> AsSpan() 
    {
        return new Span<T>(buffer, 0, Count);
    }

    public WeakEnumerator<T> GetEnumerator()
    {
        return new WeakEnumerator<T>(new Span<T>(buffer, 0, Count));
    }
}

public ref struct WeakEnumerator<T>
{
    private ReadOnlySpan<T> buffer;
    private int index;

    public T Current => buffer[index];

    public static WeakEnumerator<T> Empty => new WeakEnumerator<T>();

    public WeakEnumerator(Span<T> span) 
    {
        buffer = span;
        index = span.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool MoveNext() 
    {
        if (index > 0) 
        {
            index -= 1;
            return true;
        }

        return false;
    }

    public WeakEnumerator<T> GetEnumerator() 
    {
        return this;
    }
}

internal struct WeakComparer<T> : IComparer<T>
{
    private Comparison<T> comparison;

    public WeakComparer(Comparison<T> comparison) 
    {
        this.comparison = comparison;
    }

    public int Compare(T x, T y)
    {
        if (x == null && y == null)
            return 0;
        if (x == null)
            return 1;
        if (y == null)
            return -1;
        return comparison(x, y);
    }
}
