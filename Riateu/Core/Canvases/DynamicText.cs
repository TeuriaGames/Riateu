using System;
using MoonWorks.Graphics;
using MoonWorks.Graphics.Font;
using MoonWorks.Math.Float;
using Riateu.Graphics;

namespace Riateu;

public class DynamicText : Text
{
    private bool dirty;
    private GraphicsDevice device;
    private Font font;

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

    public int VisibleText 
    {
        get => visibleText;
        set 
        {
            if (value > text.Length) 
            {
                visibleText = text.Length;
            } 
            else 
            {
                visibleText = value;
            }


            if (visibleText <= 0) 
            {
                visibleText = 0;
                DynamicTexture = new SpriteTexture(Texture, new Rect(0, 0, 0, 0));
                return;
            }

            var span = text.AsSpan();
            var newSpan = span.Slice(0, visibleText);
            var f = font.TextBounds(newSpan, pixelSize, HorizontalAlignment.Left, 
                VerticalAlignment.Baseline, out WellspringCS.Wellspring.Rectangle rectVisible);

            DynamicTexture = new SpriteTexture(
                Texture, 
                new Rect(0, 0, 
                    (int)rectVisible.W, 
                    (int)rectVisible.H)
                );
        }
    }

    private int visibleText;
    public uint Width => (uint)rect.W;
    public uint Height => (uint)rect.H;
    private Rect rect;
    private SpriteTexture DynamicTexture;
    
    public DynamicText(GraphicsDevice device, Font font, string text, int pixel, int textVisible = -1) 
    {
        if (textVisible == -1) 
        {
            textVisible = text.Length;
        }
        this.device = device;
        this.font = font;
        Batch = new TextBatch(device);
        this.text = text;
        pixelSize = pixel;

        var f = font.TextBounds(text, pixelSize, HorizontalAlignment.Left, 
            VerticalAlignment.Baseline, out WellspringCS.Wellspring.Rectangle rect);
        this.rect.X = (int)rect.X;
        this.rect.Y = (int)rect.Y;
        this.rect.W = (int)rect.W;
        this.rect.H = (int)rect.H;
        Resubmit();

        VisibleText = textVisible;
    }

    private void Resubmit() 
    {
        CommandBuffer buffer = device.AcquireCommandBuffer();

        if (Width != Bounds.Width || Height != Bounds.Height) 
        {
            if (Texture != null) 
            {
                Texture.Dispose();
                Texture = null;
            }

            Texture = Texture.CreateTexture2D(
                device, Width, Height, 
                TextureFormat.R8G8B8A8, 
                TextureUsageFlags.Sampler | TextureUsageFlags.ColorTarget);
        }

        Bounds = new Rectangle((int)rect.X, (int)rect.Y, (int)rect.W, (int)rect.H);
        
        var matrix = Matrix4x4.CreateTranslation(0, Height, 1) 
            * Matrix4x4.CreateOrthographicOffCenter(0, Width, Height, 0, -1, 1);
        
        Batch.Start(font);
        Batch.Add(text, pixelSize, Color.White);
        Batch.UploadBufferData(buffer);

        buffer.BeginRenderPass(new ColorAttachmentInfo(Texture, Color.Transparent));
        buffer.BindGraphicsPipeline(GameContext.MSDFPipeline);
        Batch.Render(buffer, matrix);
        buffer.EndRenderPass();
        device.Submit(buffer);
    }

    public override void Draw(IBatch batch, Vector2 position) 
    {
        if (dirty) 
        {
            Resubmit();
            dirty = false;
        }
        batch.Add(DynamicTexture, Texture, GameContext.GlobalSampler, position, Matrix3x2.Identity);
    }
}