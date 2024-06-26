using System;
using MoonWorks.Graphics;
using MoonWorks.Graphics.Font;
using MoonWorks.Math.Float;
using Riateu.Graphics;

namespace Riateu;

/// <summary>
/// This type of text can be changed their property dynamically. When the property changed,
/// it will try to re-render and resubmit its texture. Becareful when using this text.
/// </summary>
public class DynamicText : Text
{
    private bool dirty;
    private GraphicsDevice device;
    private Font font;

    /// <summary>
    /// A string text for this text. When changed, it will caused to re-render and resubmit
    /// its texture.
    /// </summary>
    public string Text
    {
        get => text;
        set
        {
            if (text != value)
            {
                text = value;
                if (!touchedVisiblity)
                {
                    visibleText = text.Length;
                }
                dirty = true;
            }
        }
    }

    /// <summary>
    /// A pixel size of this font. When changed, it will caused to re-render and resubmit
    /// its texture.
    /// </summary>
    public int Pixel
    {
        get => pixel;
        set
        {
            if (pixel != value)
            {
                pixel = value;
                dirty = true;
            }
        }
    }

    /// <summary>
    /// A numeric visibled text value. When changed, it will not resubmit, instead it will
    /// just change the quad of the texture.
    /// </summary>
    public int VisibleText
    {
        get => visibleText;
        set
        {
            touchedVisiblity = true;
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
                DynamicTexture = new Quad(Texture, new Rect(0, 0, 0, 0));
                return;
            }

            var span = text.AsSpan();
            var newSpan = span.Slice(0, visibleText);
            var f = font.TextBounds(new string(newSpan), pixel, HorizontalAlignment.Left,
                VerticalAlignment.Baseline, out WellspringCS.Wellspring.Rectangle rectVisible);

            DynamicTexture = new Quad(
                Texture,
                new Rect(0, 0,
                    (int)rectVisible.W,
                    (int)rectVisible.H)
                );
        }
    }

    private bool touchedVisiblity = false;
    private int visibleText;
    private Quad DynamicTexture;
    /// <summary>
    /// The texture width of the rendered text.
    /// </summary>
    public uint Width => (uint)rect.W;
    /// <summary>
    /// The texture height of the rendered text.
    /// </summary>
    public uint Height => (uint)rect.H;
    private Rect rect;
    private string text;
    private int pixel;

    /// <summary>
    /// An initialization for this class.
    /// </summary>
    /// <param name="device">An application graphics device</param>
    /// <param name="font">A font to use for rendering text</param>
    /// <param name="text">A text that should be rendered</param>
    /// <param name="pixel">A size of the text</param>
    /// <param name="textVisible">A numeric visibled text value</param>
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
        this.pixel = pixel;

        var f = font.TextBounds(text, this.pixel, HorizontalAlignment.Left,
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
        Batch.Add(text, pixel, Color.White);
        CopyPass copyPass = buffer.BeginCopyPass();
        Batch.UploadBufferData(buffer);
        buffer.EndCopyPass(copyPass);

        RenderPass renderPass = buffer.BeginRenderPass(new ColorAttachmentInfo(Texture, false, Color.Transparent));
        renderPass.BindGraphicsPipeline(GameContext.MSDFPipeline);
        Batch.Render(renderPass, matrix);
        buffer.EndRenderPass(renderPass);
        device.Submit(buffer);
    }

    /// <inheritdoc/>
    public override void Render(Batch batch, Vector2 position)
    {
        if (dirty)
        {
            Resubmit();
            dirty = false;
        }
        batch.Draw(DynamicTexture, position, Color.White);
    }
}
