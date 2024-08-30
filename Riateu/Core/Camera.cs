using System;
using System.Numerics;
using Riateu.Graphics;

namespace Riateu;

/// <summary>
/// A class that holds the matrix, view, projection, and a viewport of the world space.
/// </summary>
/// <param name="width">A viewport width</param>
/// <param name="height">A viewport height</param>
public class Camera(int width, int height) 
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
        var zooming = new Vector3(zoom, -1);
        // Origin
        var origXy = new Vector2((int)Math.Floor(origin.X), (int)Math.Floor(origin.Y));
        var orig = new Vector3(origXy, 0);

        var model = 
            Matrix4x4.Identity               *
            Matrix4x4.CreateTranslation(pos) * 
            Matrix4x4.CreateRotationZ(angle) *
            Matrix4x4.CreateScale(zooming)   *
            Matrix4x4.CreateTranslation(orig);

        var view = Matrix4x4.CreateTranslation(0, 0, 1);
        var projection = Matrix4x4.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, -1, 1);

        transform = model * view * projection;

        Matrix4x4.Invert(model, out inverse);
        dirty = false;
    }

    public Vector2 ScreenToViewport(Vector2 position) 
    {
        int windowWidth = GameApp.Instance.Width;
        int windowHeight = GameApp.Instance.Height;
        float currentX = position.X - viewport.X;
        float currentY = position.Y - viewport.Y;

        return new Vector2(currentX / windowWidth * viewport.Width, currentY / windowHeight * viewport.Height);
    }

    public Vector2 ScreenToWorld(Vector2 position) 
    {
        return ScreenToCamera(ScreenToViewport(position));
    }

    /// <summary>
    /// Convert a screen position into a world position, such as mouse position.
    /// </summary>
    /// <param name="position">A screen position</param>
    /// <returns>A world position from the screen position</returns>
    public Vector2 ScreenToCamera(Vector2 position) 
    {
        return Vector2.Transform(position, inverse);
    }

    /// <summary>
    /// Convert a world position into a screen position.
    /// </summary>
    /// /// <param name="position">A world position</param>
    /// <returns>A screen position from the world position</returns>
    public Vector2 CameraToScreen(Vector2 position) 
    {
        return Vector2.Transform(position, transform);
    }

    /// <summary>
    /// A viewport of the world space.
    /// </summary>
    public Viewport Viewport
    {
        get => viewport;
        set => viewport = value;
    }

    /// <summary>
    /// An X axis of the position of the camera.
    /// </summary>
    public float X 
    {
        get => position.X;
        set 
        {
            dirty = true;
            position.X = value;
        }
    }

    /// <summary>
    /// A Y axis of the position of the camera.
    /// </summary>
    public float Y 
    {
        get => position.Y;
        set 
        {
            dirty = true;
            position.Y = value;
        }
    }

    public float Left 
    {
        get 
        {
            if (dirty) 
            {
                UpdateMatrix();
            }
            return Vector2.Transform(Vector2.Zero, inverse).X;
        }
    }

    public float Right
    {
        get 
        {
            if (dirty) 
            {
                UpdateMatrix();
            }
            return Vector2.Transform(Vector2.UnitX * viewport.Width, inverse).X;
        }
    }

    public float Top
    {
        get 
        {
            if (dirty) 
            {
                UpdateMatrix();
            }
            return Vector2.Transform(Vector2.Zero, inverse).Y;
        }
    }

    public float Bottom
    {
        get 
        {
            if (dirty) 
            {
                UpdateMatrix();
            }
            return Vector2.Transform(-Vector2.UnitY * viewport.Height, inverse).Y;
        }
    }

    /// <summary>
    /// The position of the camera.
    /// </summary>
    public Vector2 Position 
    {
        get => position;
        set 
        {
            dirty = true;
            position = value;
        }
    }

    /// <summary>
    /// A rotation angle of the camera. It is a z-axis rotation of the camera.
    /// </summary>
    public float Angle 
    {
        get => angle;
        set 
        {
            dirty = true;
            angle = value;
        }
    }

    /// <summary>
    /// The world transform matrix of the camera.
    /// </summary>
    public Matrix4x4 Transform 
    {
        get 
        {
            if (dirty)
                UpdateMatrix();
            return transform;
        }
    }

    /// <summary>
    /// A scale of the camera.
    /// </summary>
    public Vector2 Zoom 
    {
        get => zoom;
        set 
        {
            dirty = true;
            zoom = value;
        }
    }

    /// <summary>
    /// An offset position of the camera.
    /// </summary>
    public Vector2 Offset
    {
        get => offset;
        set 
        {
            dirty = true;
            offset = value;
        }
    }

    /// <summary>
    /// The center offset of the camera.
    /// </summary>
    public Vector2 Origin
    {
        get => origin;
        set 
        {
            dirty = true;
            origin = value;
        }
    }

    public static implicit operator Matrix4x4(Camera camera) 
    {
        return camera.Transform;
    }
}
