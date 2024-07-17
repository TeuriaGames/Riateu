using System;
using System.Collections.Generic;
using MoonWorks.Math.Float;
using Riateu.Components;

namespace Riateu.Physics;

public class SpatialHash 
{
    private int width;
    private int height;
    private int columns;
    private int rows;
    private int cellSize;
    private List<List<PhysicsComponent>> buckets = new List<List<PhysicsComponent>>();
    private HashSet<int> idsLookup = new HashSet<int>();

    public int Width => width;
    public int Height => height;
    public int Columns => columns;
    public int Rows => rows;
    public int CellSize => cellSize;

    public SpatialHash(int width, int height, int cellSize) 
    {
        this.cellSize = cellSize;
        columns = width / cellSize;
        rows = height / cellSize;

        for (int i = 0; i < columns * rows; i++) 
        {
            buckets.Add(new List<PhysicsComponent>());
        }

        this.width = width;
        this.height = height;
    }

    public void AddObject(PhysicsComponent shape) 
    {
        // TODO don't cast it
        HashSet<int> ids = GetIDs(shape.Shape);
        foreach (int id in ids) 
        {
            buckets[id].Add(shape);
        }
    }

    public List<PhysicsComponent> GetNearby(PhysicsComponent comp) 
    {
        List<PhysicsComponent> objects = new List<PhysicsComponent>();
        // TODO don't cast it
        HashSet<int> ids = GetIDs(comp.Shape);
        foreach (int id in ids) 
        {
            objects.AddRange(buckets[id]);
        }

        return objects;
    }

    private HashSet<int> GetIDs(Shape shape) 
    {
        idsLookup.Clear();

        Vector2 min = shape.Min;
        Vector2 max = shape.Max;

        float width = this.width / cellSize;
        AddBucket(new Vector2(min.X - 4, min.Y - 4), width, idsLookup);
        AddBucket(new Vector2(max.X + 4, min.Y - 4), width, idsLookup);
        AddBucket(new Vector2(max.X + 4, max.Y + 4), width, idsLookup);
        AddBucket(new Vector2(min.X - 4, max.Y + 4), width, idsLookup);

        return idsLookup;
    }

    private void AddBucket(Vector2 pos, float width, HashSet<int> idLookup) 
    {
        int cellPosition = (int)((Math.Floor(pos.X / cellSize)) + (Math.Floor(pos.Y / cellSize)) * width);

        if (cellPosition < 0 || cellPosition >= (columns * rows))
        {
            return;
        }

        idLookup.Add(cellPosition);
    }

    public void Clear() 
    {
        for (int i = 0; i < buckets.Count; i++) 
        {
            buckets[i].Clear();
        }
    }
}