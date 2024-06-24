using System;
using MoonWorks;
using MoonWorks.Graphics;
using MoonWorks.Math.Float;
using Riateu.Graphics;

namespace Riateu;

/// <summary>
/// A scene-like class but design only for composing a rendering stuff and turns it into a texture.
/// </summary>
public class Canvas : IDisposable
{
    /// <summary>
    /// A width of a canvas.
    /// </summary>
    public uint Width => width;
    /// <summary>
    /// A height of a canvas.
    /// </summary>
    public uint Height => height;

    private uint width;
    private uint height;

    /// <summary>
    /// The primary texture of a canvas.
    /// </summary>
    public Texture CanvasTexture;
    /// <summary>
    /// Check if the <see cref="Riateu.Canvas"/> is already disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// A <see cref="Riateu.Scene"/> that the <see cref="Riateu.Canvas"/> is currently on.
    /// </summary>
    public Scene Scene { get; set; }

    /// <summary>
    /// Initialization for the <see cref="Riateu.Canvas"/>.
    /// </summary>
    /// <param name="scene">A <see cref="Riateu.Scene"/> to reference with</param>
    /// <param name="device">An application graphics device</param>
    /// <param name="width">A width of a canvas</param>
    /// <param name="height">A height of a canvas</param>
    public Canvas(Scene scene, GraphicsDevice device, int width, int height) :
        this(scene, device, (uint)width, (uint)height) {}

    /// <summary>
    /// Initialization for the <see cref="Riateu.Canvas"/>.
    /// </summary>
    /// <param name="scene">A <see cref="Riateu.Scene"/> to reference with</param>
    /// <param name="device">An application graphics device</param>
    /// <param name="width">A width of a canvas</param>
    /// <param name="height">A height of a canvas</param>
    public Canvas(Scene scene, GraphicsDevice device, uint width, uint height)
    {
        CanvasTexture = Texture.CreateTexture2D(device, width, height, TextureFormat.R8G8B8A8, TextureUsageFlags.Sampler | TextureUsageFlags.ColorTarget);
        this.width = width;
        this.height = height;
        this.Scene = scene;
    }
    /// <summary>
    /// A method that called during the draw loop. Do your draw calls here.
    /// </summary>
    /// <param name="buffer">A command buffer</param>
    public virtual void Render(CommandBuffer buffer) {}

    /// <summary>
    /// Create a <see cref="Riateu.DefaultCanvas"/> to do the rendering for you.
    /// </summary>
    /// <param name="scene">A <see cref="Riateu.Scene"/> to reference with</param>
    /// <param name="device">An application graphics device</param>
    /// <returns>A <see cref="Riateu.DefaultCanvas"/></returns>
    public static DefaultCanvas CreateDefault(Scene scene, GraphicsDevice device)
    {
        return new DefaultCanvas(scene, device, scene.GameInstance.Width, scene.GameInstance.Height);
    }

    /// <summary>
    /// End of the canvas, disposed all of the resources.
    /// </summary>
    public virtual void End()
    {
        Dispose();
    }

    /// <summary>
    /// Dispose all of the resource.
    /// </summary>
    /// <param name="disposing">Dispose unmanaged resource</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!IsDisposed)
        {
            if (disposing)
            {
                CanvasTexture.Dispose();
            }

            IsDisposed = true;
        }
    }

    /// <summary>
    /// Add the vertex buffer from the <see cref="Riateu.Canvas.CanvasTexture"/>.
    /// </summary>
    /// <param name="batch">A batch system to add the canvas texture</param>
    /// <param name="sampler">The sampler for the texture</param>
    public void ApplyCanvasToBatch(IBatch batch, Sampler sampler)
    {
        batch.Add(CanvasTexture, sampler, Vector2.Zero, Color.White);
    }

    ///
    ~Canvas()
    {
#if DEBUG
        Logger.LogWarn($"The type {this.GetType()} has not been disposed properly.");
#endif
        Dispose(disposing: false);
    }

    /// <summary>
    /// Dispose all resources including unmanaged resource.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// A <see cref="Riateu.DefaultCanvas"/> to do the rendering for you.
/// </summary>
public class DefaultCanvas : Canvas
{
    private Rect scissorRect;

    /// <inheritdoc/>
    public DefaultCanvas(Scene scene, GraphicsDevice device, int width, int height) : base(scene, device, width, height)
    {
        scissorRect = new Rect(0, 0, width, height);
    }

    /// <inheritdoc/>
    public override void Render(CommandBuffer buffer) {}
}
