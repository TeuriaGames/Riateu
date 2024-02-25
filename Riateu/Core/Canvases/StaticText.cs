using System;
using MoonWorks.Graphics;
using MoonWorks.Graphics.Font;
using MoonWorks.Math;
using MoonWorks.Math.Float;
using Riateu.Graphics;

namespace Riateu;

/// <summary>
/// This type of text cannot be changed their property dynamically. It is readonly text
/// that can be rendered.
/// </summary>
public class StaticText : Text
{
    /// <summary>
    /// The immutable string text of this text.
    /// </summary>
    public string Text => text;
    private string text;

    /// <summary>
    /// An initialization for this class.
    /// </summary>
    /// <param name="device">An application graphics device</param>
    /// <param name="font">A font to use for rendering text</param>
    /// <param name="text">A text that should be rendered</param>
    /// <param name="pixel">A size of the text</param>
    /// <param name="textVisible">A numeric visibled text value</param>
    public StaticText(GraphicsDevice device, Font font, string text, int pixel, int textVisible = -1) 
    {
        if (textVisible == -1) 
        {
            textVisible = text.Length;
        }
        textVisible = MathHelper.Clamp(textVisible, 0, text.Length);
        CommandBuffer buffer = device.AcquireCommandBuffer();
        this.text = text;

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
        buffer.BeginCopyPass();
        Batch.UploadBufferData(buffer);
        buffer.EndCopyPass();

        buffer.BeginRenderPass(new ColorAttachmentInfo(Texture, Color.Transparent));
        buffer.BindGraphicsPipeline(GameContext.MSDFPipeline);
        Batch.Render(buffer, matrix);
        buffer.EndRenderPass();
        device.Submit(buffer);
    }

    /// <inheritdoc/>
    public override void Draw(IBatch batch, Vector2 position) 
    {
        batch.Add(Texture, GameContext.GlobalSampler, position, Color.White, Matrix3x2.Identity);
    }
}
