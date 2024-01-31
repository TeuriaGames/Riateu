using System;
using MoonWorks;
using MoonWorks.Graphics;
using MoonWorks.Math.Float;
using Riateu.Graphics;

namespace Riateu;

public class Canvas : IDisposable
{
    public uint Width => width;
    public uint Height => height;

    private uint width;
    private uint height;

    public Texture CanvasTexture;
    public bool IsDisposed { get; private set; }

    public Scene Scene { get; set; }

    public Canvas(Scene scene, GraphicsDevice device, int width, int height) :
        this(scene, device, (uint)width, (uint)height) {}

    public Canvas(Scene scene, GraphicsDevice device, uint width, uint height) 
    {
        CanvasTexture = Texture.CreateTexture2D(device, width, height, TextureFormat.R8G8B8A8, TextureUsageFlags.Sampler | TextureUsageFlags.ColorTarget);
        this.width = width;
        this.height = height;
        this.Scene = scene;
    }

    public virtual void BeforeDraw(CommandBuffer buffer, IBatch batch) {}
    public virtual void Draw(CommandBuffer buffer, IBatch batch) 
    {
    }
    public virtual void AfterDraw(CommandBuffer buffer, IBatch batch) {}

    public static Canvas CreateDefault(Scene scene, GraphicsDevice device) 
    {
        return new DefaultCanvas(scene, device, scene.GameInstance.Width, scene.GameInstance.Height);
    }

    public virtual void End() 
    {
        Dispose();
    }

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
        batch.Add(CanvasTexture, sampler, Vector2.Zero, Matrix3x2.Identity);
    }

    ///
    ~Canvas()
    {
#if DEBUG
        Logger.LogWarn($"The type {this.GetType()} has not been disposed properly.");
#endif
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

public class DefaultCanvas : Canvas
{
    private Rect scissorRect;
    public DefaultCanvas(Scene scene, GraphicsDevice device, int width, int height) : base(scene, device, width, height)
    {
        scissorRect = new Rect(0, 0, width, height);
    }

    public override void Draw(CommandBuffer buffer, IBatch batch)
    {
        batch.Start();
        Scene.EntityList.Draw(buffer, batch);
        batch.End(buffer);

        buffer.BeginRenderPass(new ColorAttachmentInfo(CanvasTexture, Color.Transparent));
        buffer.BindGraphicsPipeline(GameContext.DefaultPipeline);
        buffer.SetScissor(scissorRect);
        batch.Draw(buffer);
        buffer.EndRenderPass();
    }
}