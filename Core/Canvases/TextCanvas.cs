using System;
using MoonWorks.Graphics;
using MoonWorks.Graphics.Font;
using MoonWorks.Math.Float;
using Riateu.Graphics;

namespace Riateu;

// FIXME I don't want this all, but this is the only way to get the unique positions


public class StaticText 
{
    public Texture Texture;
    private int pixelSize;
    private string text;
    private Rectangle bounds;
    public Rectangle Bounds => bounds;

    public StaticText(GraphicsDevice device, Font font, string text, int pixel) 
    {
        CommandBuffer buffer = device.AcquireCommandBuffer();
        this.text = text;
        pixelSize = pixel;

        var f = font.TextBounds(text, pixel, HorizontalAlignment.Left, 
            VerticalAlignment.Baseline, out WellspringCS.Wellspring.Rectangle rect);
        bounds = new Rectangle((int)rect.X, (int)rect.Y, (int)rect.W, (int)rect.H);

        uint width = (uint)rect.W;
        uint height = (uint)rect.H;

        Texture = Texture.CreateTexture2D(
            device, width, height, 
            TextureFormat.R8G8B8A8, TextureUsageFlags.Sampler | TextureUsageFlags.ColorTarget);
        
        var matrix = Matrix4x4.CreateTranslation(0, height, 1) 
            * Matrix4x4.CreateOrthographicOffCenter(0, width, height, 0, -1, 1);
        
        var textBatch = new TextBatch(device);
        textBatch.Start(font);
        textBatch.Add(text, pixel, Color.White);
        textBatch.UploadBufferData(buffer);

        buffer.BeginRenderPass(new ColorAttachmentInfo(Texture, Color.Transparent));
        buffer.BindGraphicsPipeline(GameContext.MSDFPipeline);
        textBatch.Render(buffer, matrix);
        buffer.EndRenderPass();
        device.Submit(buffer);
        device.Wait();

        textBatch.Dispose();
    }

    public void Draw(Batch batch, Vector2 position) 
    {
        batch.Add(Texture, GameContext.GlobalSampler, position, Matrix3x2.Identity);
    }
}

public class DynamicText 
{
    public Texture Texture;
    private int pixelSize;
    private string text;
    private bool dirty;
    private TextBatch textBatch;
    private GraphicsDevice device;
    private Font font;
    private Rectangle bounds;
    public Rectangle Bounds => bounds;

    public string Text 
    {
        get => text;
        set 
        {
            if (text != value) 
            {
                text = value;
                dirty = true;
            }
        }
    }
    
    public DynamicText(GraphicsDevice device, Font font, string text, int pixel) 
    {
        this.device = device;
        this.font = font;
        textBatch = new TextBatch(device);
        this.text = text;
        pixelSize = pixel;
        Resubmit();
    }

    private void Resubmit() 
    {
        CommandBuffer buffer = device.AcquireCommandBuffer();

        var f = font.TextBounds(text, pixelSize, HorizontalAlignment.Left, 
            VerticalAlignment.Baseline, out WellspringCS.Wellspring.Rectangle rect);
        bounds = new Rectangle((int)rect.X, (int)rect.Y, (int)rect.W, (int)rect.H);

        uint width = (uint)rect.W;
        uint height = (uint)rect.H;

        if (Texture != null) 
        {
            Texture.Dispose();
            Texture = null;
        }

        Texture = Texture.CreateTexture2D(
            device, width, height, 
            TextureFormat.R8G8B8A8, TextureUsageFlags.Sampler | TextureUsageFlags.ColorTarget);
        
        var matrix = Matrix4x4.CreateTranslation(0, height, 1) 
            * Matrix4x4.CreateOrthographicOffCenter(0, width, height, 0, -1, 1);
        
        textBatch.Start(font);
        textBatch.Add(text, pixelSize, Color.White);
        textBatch.UploadBufferData(buffer);

        buffer.BeginRenderPass(new ColorAttachmentInfo(Texture, Color.Transparent));
        buffer.BindGraphicsPipeline(GameContext.MSDFPipeline);
        textBatch.Render(buffer, matrix);
        buffer.EndRenderPass();
        device.Submit(buffer);
        device.Wait();
    }

    public void Draw(Batch batch, Vector2 position) 
    {
        if (dirty) 
        {
            Resubmit();
            dirty = false;
        }
        batch.Add(Texture, GameContext.GlobalSampler, position, Matrix3x2.Identity);
    }
}