using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Riateu.ECS;

// Kinda like zig *anyopaque
public unsafe class AnyOpaqueArray : IDisposable
{
    private nint elements;

    private int capacity;
    private int elementSize;
    private int count;
    private bool disposedValue;

    public int Count => count;
    public int Capacity => capacity;
    public int ElementSize => elementSize;

    public AnyOpaqueArray(int elementSize) 
    {
        capacity = 16;
        this.elementSize = elementSize;
        elements = (nint)NativeMemory.Alloc((nuint) (capacity * elementSize));
    }

    public void Set<T>(int index, in T element) 
    where T : unmanaged
    {
        Unsafe.Write<T>((void*)(elements + elementSize * index), element);
    }

    public void Add<T>(in T element) 
    where T : unmanaged
    {
        if (count >= capacity) 
        {
            capacity *= 2;
            elements = (nint)NativeMemory.Realloc((void*)elements, (nuint) (capacity * elementSize));
        }

        ((T*)elements)[count] = element;
        count++;
    }

    public void Remove(int index) 
    {
        if (index != count - 1) 
        {
            NativeMemory.Copy(
                (void*)(elements + (count - 1) * elementSize),
                (void*)(elements + index * elementSize),
                (nuint)elementSize
            );
        }

        count--;
    }

    public void Clear() 
    {
        count = 0;
    }

    public ref T Get<T>(int i) 
    where T : unmanaged
    {
        return ref ((T*)elements)[i];
    }

    public ReadOnlySpan<T> AsSpan<T>() 
    where T : unmanaged
    {
        return new ReadOnlySpan<T>((T*)elements, count);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            NativeMemory.Free((void*)elements);
            disposedValue = true;
        }
    }

    ~AnyOpaqueArray()
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