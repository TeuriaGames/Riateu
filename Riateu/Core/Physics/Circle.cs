using System;
using System.Numerics;
using Riateu.Graphics;

namespace Riateu.Physics;

public class Circle : Shape
{
    public override Vector2 AbsoluteMin => new Vector2(Entity.PosX + BoundingBox.X, Entity.PosY + BoundingBox.Y);
    public override Vector2 AbsoluteMax => new Vector2(Entity.PosX + BoundingBox.X + BoundingBox.Width, Entity.PosY + BoundingBox.Y + BoundingBox.Height);

    public Vector2 Position;
    public float Radius;

    public Circle(Entity entity, Vector2 position, float radius) : base(
        entity, 
        new RectangleF(
            position.X - radius,
            position.Y - radius,
            radius * radius,
            radius * radius
        )
    )
    {
    }

    public override Shape Clone()
    {
        return new Circle(Entity, Position, Radius);
    }

    public override bool Collide(Vector2 position, Rectangle rect)
    {
        return Collide(position, rect.ToFloat());
    }

    public override bool Collide(Vector2 position, Vector2 point)
    {
        return false;
    }

    public override bool Collide(Vector2 position, Point point)
    {
        return false;
    }

    public override bool Collide(Vector2 position, Shape shape)
    {
        RectangleF rectF = shape.AbsoluteBoundingBox;
        return Collide(position, rectF);
    }

    public override bool Collide(Vector2 position, RectangleF rect)
    {
        float circDistX = Math.Abs(Position.X - rect.X);
        float circDistY = Math.Abs(Position.Y - rect.Y);

        if (circDistX > (rect.Width * 0.5f + Radius) || circDistY > (rect.Height * 0.5f + Radius)) 
        {
            return false;
        }

        if (circDistX <= (rect.Width * 0.5f)) 
        {
            return true;
        }

        if (circDistY <= (rect.Height * 0.5f)) 
        {
            return true;
        }

        float cornerDistance = 
            ((circDistX - rect.Width * 0.5f)  * (circDistX - rect.Width * 0.5f)) + 
            ((circDistY - rect.Height * 0.5f) * (circDistY - rect.Height * 0.5f));
        return cornerDistance <= Radius * Radius;
    }
}