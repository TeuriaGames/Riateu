using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Riateu.Components;
using Riateu.Graphics;

namespace Riateu.Physics;

public class SpatialHash 
{
    private int cellSize;
    private float inverseCellSize;
    private SpatialBucket bucket = new SpatialBucket();
    private HashSet<PhysicsComponent> temp = new HashSet<PhysicsComponent>();

    public int CellSize => cellSize;

    public SpatialHash(int cellSize = 80) 
    {
        this.cellSize = cellSize;
        inverseCellSize = 1f / cellSize;
    }

    public void AddCollider(PhysicsComponent shape) 
    {
        int left = (int)(Math.Floor(shape.Entity.PosX) * inverseCellSize);
        int top = (int)(Math.Floor(shape.Entity.PosY) * inverseCellSize);
        int right = (int)(Math.Floor(shape.Entity.PosX + shape.Shape.BoundingBox.Width) * inverseCellSize);
        int bottom = (int)(Math.Floor(shape.Entity.PosY + shape.Shape.BoundingBox.Height) * inverseCellSize);

        for (int x = left; x <= right; x++) 
        {
            for (int y = top; y <= bottom; y++) 
            {
                List<PhysicsComponent> cell = GetCell(x, y);
                cell.Add(shape);
            }
        }
    }

    public void RemoveCollider(PhysicsComponent shape) 
    {
        bucket.RemoveToBucket(shape);
    }

    public List<PhysicsComponent> GetCell(int x, int y) 
    {
        if (bucket.GetSome(x, y, out List<PhysicsComponent> colliders)) 
        {
            return colliders;
        }

        List<PhysicsComponent> newCell = new List<PhysicsComponent>();
        bucket.AddToBucket(x, y, newCell);
        return newCell;
    }

    public HashSet<PhysicsComponent> Retrieve(in Rectangle rectangle, PhysicsComponent self) 
    {
        temp.Clear();

        int left = (int)(rectangle.Left * inverseCellSize);
        int top = (int)(rectangle.Top * inverseCellSize);
        int right = (int)(rectangle.Right * inverseCellSize);
        int bottom = (int)(rectangle.Bottom * inverseCellSize);

        for (int x = left; x <= right; x++) 
        {
            for (int y = top; y <= bottom; y++)  
            {
                List<PhysicsComponent> cell = GetCell(x, y);
                if (cell == null || cell.Count <= 1) 
                {
                    continue;
                }

                Span<PhysicsComponent> cellSpan = CollectionsMarshal.AsSpan(cell);
                for (int i = 0; i < cellSpan.Length; i++)
                {
                    PhysicsComponent component = cellSpan[i];
                    if (component == self) 
                    {
                        continue;
                    }
                    
                    temp.Add(component);
                }
            }
        }


        return temp;
    }

    public void Clear() 
    {
        bucket.Clear();
    }
}

public class SpatialBucket 
{
    private Dictionary<long, List<PhysicsComponent>> bucketColliders = new Dictionary<long, List<PhysicsComponent>>();

    public void AddToBucket(int x, int y, List<PhysicsComponent> colliders) 
    {
        bucketColliders.Add(HashPos(x, y), colliders);
    }

    public void RemoveToBucket(PhysicsComponent collider) 
    {
        foreach (List<PhysicsComponent> components in bucketColliders.Values) 
        {
            if (components.Contains(collider)) 
            {
                components.Remove(collider);
            }
        }
    }

    public bool GetSome(int x, int y, out List<PhysicsComponent> colliders) 
    {
        return bucketColliders.TryGetValue(HashPos(x, y), out colliders);
    }

    public HashSet<PhysicsComponent> GetAllColliders() 
    {
        HashSet<PhysicsComponent> set = new HashSet<PhysicsComponent>();
        foreach (List<PhysicsComponent> colliders in bucketColliders.Values) 
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