using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Riateu.Components;
using Riateu.Graphics;

namespace Riateu.Physics;

public class SpatialHash 
{
    private int cellSize;
    private int offset;
    private float inverseCellSize;
    private SpatialBucket bucket = new SpatialBucket();
    private SpatialResult result = new SpatialResult();

    public int CellSize => cellSize;

    public SpatialHash(int cellSize = 80, int offset = 0) 
    {
        this.offset = offset;
        this.cellSize = cellSize;
        inverseCellSize = 1f / cellSize;
    }

    public void AddCollider(Collision shape) 
    {
        int left = (int)(Math.Floor(shape.Entity.PosX + offset) * inverseCellSize);
        int top = (int)(Math.Floor(shape.Entity.PosY + offset) * inverseCellSize);
        int right = (int)(Math.Floor(shape.Entity.PosX + shape.Shape.BoundingBox.Width + offset) * inverseCellSize);
        int bottom = (int)(Math.Floor(shape.Entity.PosY + shape.Shape.BoundingBox.Height + offset) * inverseCellSize);

        for (int x = left; x <= right; x++) 
        {
            for (int y = top; y <= bottom; y++) 
            {
                List<Collision> cell = GetCell(x, y);
                cell.Add(shape);
            }
        }
    }

    public void RemoveCollider(Collision shape) 
    {
        bucket.RemoveToBucket(shape);
    }

    public List<Collision> GetCell(int x, int y) 
    {
        if (bucket.GetSome(x, y, out List<Collision> colliders)) 
        {
            return colliders;
        }

        List<Collision> newCell = new List<Collision>();
        bucket.AddToBucket(x, y, newCell);
        return newCell;
    }

    public SpatialResult Retrieve(in Rectangle rectangle, Collision self, ulong tags) 
    {
        HashSet<Collision> temp = result.Obtain();

        int left = (int)((rectangle.Left + offset) * inverseCellSize);
        int top = (int)((rectangle.Top + offset) * inverseCellSize);
        int right = (int)((rectangle.Right + offset) * inverseCellSize);
        int bottom = (int)((rectangle.Bottom + offset) * inverseCellSize);

        for (int x = left; x <= right; x++) 
        {
            for (int y = top; y <= bottom; y++)  
            {
                List<Collision> cell = GetCell(x, y);
                if (cell == null || cell.Count <= 1) 
                {
                    continue;
                }

                for (int i = 0; i < cell.Count; i++)
                {
                    Collision component = cell[i];
                    if (component == self || (component.Tags & tags) == 0) 
                    {
                        continue;
                    }
                    
                    temp.Add(component);
                }
            }
        }

        return result;
    }

    public SpatialResult Retrieve(in Rectangle rectangle, Collision self) 
    {
        HashSet<Collision> temp = result.Obtain();

        int left = (int)((rectangle.Left + offset) * inverseCellSize);
        int top = (int)((rectangle.Top + offset) * inverseCellSize);
        int right = (int)((rectangle.Right + offset) * inverseCellSize);
        int bottom = (int)((rectangle.Bottom + offset) * inverseCellSize);

        for (int x = left; x <= right; x++) 
        {
            for (int y = top; y <= bottom; y++)  
            {
                List<Collision> cell = GetCell(x, y);
                if (cell == null || cell.Count <= 1) 
                {
                    continue;
                }

                Span<Collision> cellSpan = CollectionsMarshal.AsSpan(cell);
                for (int i = 0; i < cellSpan.Length; i++)
                {
                    Collision component = cellSpan[i];
                    if (component == self) 
                    {
                        continue;
                    }
                    
                    temp.Add(component);
                }
            }
        }

        return result;
    }

    public void Clear() 
    {
        bucket.Clear();
    }
}

public class SpatialResult : IDisposable
{
    // this is not a pool
    private ConcurrentStack<HashSet<Collision>> colliders = new ConcurrentStack<HashSet<Collision>>();
    private HashSet<Collision> current;
    internal SpatialResult() {}

    public HashSet<Collision> Obtain() 
    {
        if (colliders.TryPop(out var res)) 
        {
            res.Clear();
            return current = res;
        }

        return current = new HashSet<Collision>();
    }

    public void Pop(HashSet<Collision> components) 
    {
        if (colliders.TryPop(out var current)) 
        {
            this.current = current;
            colliders.Push(components);
        }
    }

    public IEnumerator<Collision> GetEnumerator() 
    {
        return current.GetEnumerator();
    }


    public void Dispose()
    {
        Pop(current);
    }
}

public class SpatialBucket 
{
    private Dictionary<long, List<Collision>> bucketColliders = new Dictionary<long, List<Collision>>();

    public void AddToBucket(int x, int y, List<Collision> colliders) 
    {
        bucketColliders.Add(HashPos(x, y), colliders);
    }

    public void RemoveToBucket(Collision collider) 
    {
        foreach (List<Collision> components in bucketColliders.Values) 
        {
            if (components.Contains(collider)) 
            {
                components.Remove(collider);
            }
        }
    }

    public bool GetSome(int x, int y, out List<Collision> colliders) 
    {
        return bucketColliders.TryGetValue(HashPos(x, y), out colliders);
    }

    public HashSet<Collision> GetAllColliders() 
    {
        HashSet<Collision> set = new HashSet<Collision>();
        foreach (List<Collision> colliders in bucketColliders.Values) 
        {
            set.UnionWith(colliders);
        }
        return set;
    }

    public void Clear() 
    {
        bucketColliders.Clear();
    }

    public long HashPos(int x, int y) 
    {
        return unchecked((long)x << 32 | (uint)y);
    }
}