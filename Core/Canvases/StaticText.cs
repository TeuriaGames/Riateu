using MoonWorks.Graphics;
using MoonWorks.Graphics.Font;
using MoonWorks.Math.Float;
using Riateu.Graphics;

namespace Riateu;

public class StaticText : Text
{
    public StaticText(GraphicsDevice device, Font font, string text, int pixel) 
    {
        CommandBuffer buffer = device.AcquireCommandBuffer();
        this.text = text;
        pixelSize = pixel;

        var f = font.TextBounds(text, pixel, HorizontalAlignment.Left, 
            VerticalAlignment.Baseline, out WellspringCS.Wellspring.Rectangle rect);
        Bounds = new Rectangle((int)rect.X, (int)rect.Y, (int)rect.W, (int)rect.H);

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

        textBatch.Dispose();
    }

    public override void Draw(Batch batch, Vector2 position) 
    {
        batch.Add(Texture, GameContext.GlobalSampler, position, Matrix3x2.Identity);
    }
}