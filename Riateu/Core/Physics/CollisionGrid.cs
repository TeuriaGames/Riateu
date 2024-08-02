using System.Numerics;
using Riateu.Graphics;

namespace Riateu.Physics;

public sealed class CollisionGrid : Shape
{
    public Array2D<bool> Grid;
    public int CellWidth { get; private set; }
    public int CellHeight { get; private set; }
    public int CellX => Grid.Columns;
    public int CellY => Grid.Rows;

    public override Vector2 Min => new Vector2(Entity.PosX + BoundingBox.X, Entity.PosY + BoundingBox.Y);
    public override Vector2 Max => new Vector2(Entity.PosX + BoundingBox.X + BoundingBox.Width, Entity.PosY + BoundingBox.Y + BoundingBox.Height);


    public CollisionGrid(
        Entity entity, 
        int cellsX, int cellsY, int cellWidth, int cellHeight) : base(entity, new Rectangle(0, 0, cellsX * cellWidth, cellsY * cellHeight))
    {
        Grid = new Array2D<bool>(cellsX, cellsY);

        CellWidth = cellWidth;
        CellHeight = cellHeight;
    }


    public CollisionGrid(Entity entity, int cellWidth, int cellHeight, Array2D<bool> grid) : base(entity, new Rectangle(0, 0, grid.Rows * cellWidth, grid.Columns * cellHeight))
    {
        Grid = grid;
        CellWidth = cellWidth;
        CellHeight = cellHeight;
    }
    public CollisionGrid(Entity entity, int cellWidth, int cellHeight, string[,] characters) : base(
        entity, new Rectangle(0, 0, characters.GetLength(1) * cellWidth, characters.GetLength(0) * cellHeight))
    {
        var columns = characters.GetLength(0);
        var rows = characters.GetLength(1);
        Grid = new Array2D<bool>(rows, columns);
        CellWidth = cellWidth;
        CellHeight = cellHeight;
        for (int x = 0; x < rows; x++) 
            for (int y = 0; y < columns; y++) 
            {
                if (characters[x, y] == "0") 
                {
                    Grid[x, y] = false;
                    continue;
                } 
                Grid[x, y] = true;
            }
    }

    public CollisionGrid(Entity entity, int cellWidth, int cellHeight, Array2D<string> characters) 
        : base(entity, new Rectangle(0, 0, characters.Rows * cellWidth, characters.Columns * cellHeight))
    {
        var columns = characters.Columns;
        var rows = characters.Rows;
        CellWidth = cellWidth;
        CellHeight = cellHeight;
        Grid = new Array2D<bool>(rows, columns);
        for (int x = 0; x < rows; x++) 
        {
            for (int y = 0; y < columns; y++) 
            {
                if (characters[x, y] == "0") 
                {
                    Grid[x, y] = false;
                    continue;
                }
                Grid[x, y] = true;
            }
        }
    }

    public CollisionGrid(Entity entity, int cellWidth, int cellHeight, int columns, int rows, string[] characters) 
        : base(entity, new Rectangle(0, 0, rows * cellWidth, columns * cellHeight))
    {
        var characters2D = StackArray2D<string>.FromArray(rows, columns, characters);
        Grid = new Array2D<bool>(rows, columns);
        CellWidth = cellWidth;
        CellHeight = cellHeight;
        for (int x = 0; x < rows; x++) 
            for (int y = 0; y < rows; y++) 
            {
                if (characters2D[x, y] == "0") 
                {
                    Grid[x, y] = false;
                    continue;
                } 
                Grid[x, y] = true;
            }
    }

    public void ChangeGrid(Array2D<bool> grid) 
    {
        Grid = grid;
        BoundingBox = new Rectangle(0, 0, grid.Rows * CellWidth, grid.Columns * CellHeight);
    }

    public Rectangle GetAbsoluteBounds() 
    {
        return new Rectangle(
            (int)Entity.Position.X + BoundingBox.X,
            (int)Entity.Position.Y + BoundingBox.Y,
            BoundingBox.Width,
            BoundingBox.Height
        );
    }

    public override bool Collide(Vector2 position, Rectangle rect)
    {
        var x = (int)((rect.Left + position.X - Entity.PosX + 1.0f) / CellWidth);
        var y = (int)((rect.Top + position.Y - Entity.PosY + 1.0f) / CellHeight);

        var width = (int)((rect.Right - Entity.PosX - 1.0f) / CellWidth) - x + 1;
        var height = (int)((rect.Bottom - Entity.PosY - 1.0f) / CellHeight) - y + 1;

        if (x < 0) 
        {
            width += x;
            x = 0;
        }
        if (y < 0) 
        {
            height += y;
            y = 0;
        }
        if (x + width > CellY) { width = CellY - x; }
        if (y + height > CellX) { height = CellX - y; }

        for (int xa = 0; xa < width; xa++) 
            for (int ya = 0; ya < height; ya++) 
            {
                if (Grid[x + xa, y + ya]) { return true; }
            }

        return false;
    }

    public override bool Collide(Vector2 position, AABB aabb)
    {
        if (!aabb.Collide(Vector2.Zero, BoundingBox)) { return false; }
        return Collide(position, aabb.GetAbsoluteBounds(position));
    }

    public override bool Collide(Vector2 position, Vector2 value)
    {
        if (value.X >= Entity.PosX && value.Y >= Entity.PosY && 
            value.X < Entity.PosX + BoundingBox.Width && value.Y < Entity.PosY + BoundingBox.Height) 
        {
            var indexX = (int)((value.X - Entity.PosX) / CellWidth);
            var indexY = (int)((value.Y - Entity.PosY) / CellHeight);
            return Grid[indexX, indexY];
        }
        return false;
    }

    public override void DebugDraw(CommandBuffer buffer, Batch draw)
    {
        // for (int x = 0; x < CellY; x++)
        //     for (int y = 0; y < CellX; y++)
        //     {
        //         if (!Grid[x, y])
        //             continue;
        //         Canvas.DrawRect(
        //             spriteBatch,
        //             (int)GlobalLeft + x * CellWidth, (int)GlobalTop + y * CellHeight,
        //             CellWidth, CellHeight, 1, Color.Red);
        //     }
    }

    public override Shape Clone()
    {
        return new CollisionGrid(Entity, CellWidth, CellHeight, Grid.Clone());
    }

    public void UpdateTile(Point pixel, bool collidable) 
    {
        Grid[pixel.X, pixel.Y] = collidable;
    }

    public override bool Collide(Vector2 position, Point point)
    {
        return false;
    }

    public override bool Collide(Vector2 position, CollisionGrid grid)
    {
        return false;
    }
}