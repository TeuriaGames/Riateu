using MoonWorks.Graphics;
using Riateu.Graphics;

namespace Riateu;

public class Canvas 
{
    public uint Width => width;
    public uint Height => height;

    private uint width;
    private uint height;

    public Texture CanvasTexture;

    public Scene Scene { get; set; }

    public Canvas(Scene scene, GraphicsDevice device, int width, int height) :
        this(scene, device, (uint)width, (uint)height)
    {

    }

    public Canvas(Scene scene, GraphicsDevice device, uint width, uint height) 
    {
        CanvasTexture = Texture.CreateTexture2D(device, width, height, TextureFormat.R8G8B8A8, TextureUsageFlags.Sampler | TextureUsageFlags.ColorTarget);
        this.width = width;
        this.height = height;
        this.Scene = scene;
    }

    public virtual void BeforeDraw(ref CommandBuffer buffer, Batch batch) {}
    public virtual void Draw(ref CommandBuffer buffer, Batch batch) 
    {
    }
    public virtual void AfterDraw(ref CommandBuffer buffer, Batch batch) {}

    public static Canvas CreateDefault(Scene scene, GraphicsDevice device) 
    {
        return new DefaultCanvas(scene, device, scene.GameInstance.Width, scene.GameInstance.Height);
    }
}

public class DefaultCanvas : Canvas
{
    private Rect scissorRect;
    public DefaultCanvas(Scene scene, GraphicsDevice device, int width, int height) : base(scene, device, width, height)
    {
        scissorRect = new Rect(0, 0, width, height);
    }

    public override void Draw(ref CommandBuffer buffer, Batch batch)
    {
        foreach (var entity in Scene.EntityList) 
        {
            entity.Draw(batch);
        }    
        batch.PushVertex(buffer);

        buffer.BeginRenderPass(new ColorAttachmentInfo(CanvasTexture, Color.Transparent));
        buffer.BindGraphicsPipeline(GameContext.DefaultPipeline);
        buffer.SetScissor(scissorRect);
        batch.Draw(buffer);
        buffer.EndRenderPass();
    }
}