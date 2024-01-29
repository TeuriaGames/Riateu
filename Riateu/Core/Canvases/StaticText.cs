using System;
using MoonWorks.Graphics;
using MoonWorks.Graphics.Font;
using MoonWorks.Math;
using MoonWorks.Math.Float;
using Riateu.Graphics;

namespace Riateu;

public class StaticText : Text
{
    public StaticText(GraphicsDevice device, Font font, string text, int pixel, int textVisible = -1) 
    {
        textVisible = MathHelper.Clamp(textVisible, 0, text.Length);
        CommandBuffer buffer = device.AcquireCommandBuffer();
        this.text = text;
        pixelSize = pixel;

        var textSpan = text.AsSpan();
        var textSpanSliced = textSpan.Slice(0, textVisible);

        var f = font.TextBounds(textSpanSliced, pixel, HorizontalAlignment.Left, 
            VerticalAlignment.Baseline, out WellspringCS.Wellspring.Rectangle rect);
        Bounds = new Rectangle((int)rect.X, (int)rect.Y, (int)rect.W, (int)rect.H);

        uint width = (uint)rect.W;
        uint height = (uint)rect.H;

        Texture = Texture.CreateTexture2D(
            device, width, height, 
            TextureFormat.R8G8B8A8, TextureUsageFlags.Sampler | TextureUsageFlags.ColorTarget);
        
        var matrix = Matrix4x4.CreateTranslation(0, height, 1) 
            * Matrix4x4.CreateOrthographicOffCenter(0, width, height, 0, -1, 1);
        
        Batch = new TextBatch(device);
        Batch.Start(font);
        Batch.Add(text, pixel, Color.White);
        Batch.UploadBufferData(buffer);

        buffer.BeginRenderPass(new ColorAttachmentInfo(Texture, Color.Transparent));
        buffer.BindGraphicsPipeline(GameContext.MSDFPipeline);
        Batch.Render(buffer, matrix);
        buffer.EndRenderPass();
        device.Submit(buffer);
    }

    public override void Draw(IBatch batch, Vector2 position) 
    {
        batch.Add(Texture, GameContext.GlobalSampler, position, Matrix3x2.Identity);
    }
}
