using System;
using MoonWorks.Graphics;
using MoonWorks.Math.Float;

namespace Riateu;

public struct Camera(int width, int height) 
{
    private Matrix4x4 transform = Matrix4x4.Identity;
    private Matrix4x4 inverse = Matrix4x4.Identity;
    private float angle;

    private Vector2 position;
    private Vector2 zoom = Vector2.One;
    private Vector2 origin;
    private Vector2 offset;

    private bool dirty = true;

    private Viewport viewport = new Viewport(width, height);

    private void UpdateMatrix() 
    {
        // Position
        var xy = new Vector2((int)Math.Floor(position.X + offset.X), (int)Math.Floor(position.Y + offset.Y));
        var pos = new Vector3(xy, 0);
        // Zoom
        var zooming = new Vector3(zoom, 1);
        // Origin
        var origXy = new Vector2((int)Math.Floor(origin.X), (int)Math.Floor(origin.Y));
        var orig = new Vector3(origXy, 0);

        var model = 
            Matrix4x4.Identity               *
            Matrix4x4.CreateTranslation(pos) * 
            Matrix4x4.CreateRotationZ(angle) *
            Matrix4x4.CreateScale(zooming)   *
            Matrix4x4.CreateTranslation(orig);

        var view = Matrix4x4.CreateTranslation(0, 0, 0);
        var projection = Matrix4x4.CreateOrthographicOffCenter(0, width, 0, height, -1, 1);

        transform = model * view * projection;

        Matrix4x4.Invert(ref transform, out inverse);
        dirty = false;
    }

    public readonly Vector2 ScreenToCamera(Vector2 position) 
    {
        return Vector2.Transform(position, inverse);
    }

    public readonly Vector2 CameraToScreen(Vector2 position) 
    {
        return Vector2.Transform(position, transform);
    }

    public Viewport Viewport
    {
        get => viewport;
        set => viewport = value;
    }

    public float X 
    {
        get => position.X;
        set 
        {
            dirty = true;
            position.X = value;
        }
    }

    public float Y 
    {
        get => position.Y;
        set 
        {
            dirty = true;
            position.Y = value;
        }
    }

    public Vector2 Position 
    {
        get => position;
        set 
        {
            dirty = true;
            position = value;
        }
    }

    public float Angle 
    {
        get => angle;
        set 
        {
            dirty = true;
            angle = value;
        }
    }

    public Matrix4x4 Transform 
    {
        get 
        {
            if (dirty)
                UpdateMatrix();
            return transform;
        }
    }

    public Vector2 Zoom 
    {
        get => zoom;
        set 
        {
            dirty = true;
            zoom = value;
        }
    }

    public Vector2 Offset
    {
        get => offset;
        set 
        {
            dirty = true;
            offset = value;
        }
    }

    public Vector2 Origin
    {
        get => origin;
        set 
        {
            dirty = true;
            origin = value;
        }
    }
}