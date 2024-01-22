using System;
using MoonWorks.Graphics;
using MoonWorks.Graphics.Font;
using MoonWorks.Math.Float;
using Riateu.Graphics;

namespace Riateu;

// FIXME I don't want this all, but this is the only way to get the unique positions


public class Text 
{
    public Texture Texture;
    public Matrix4x4 Matrix;
    public Vector2 Position;
    public string TextString;
    public int PixelSize;

    public Text(TextCanvas canvas, string text, int pixel, Vector2 position) 
    {
        this.TextString = text;
        this.PixelSize = pixel;
        Position = position;

        Texture = Texture.CreateTexture2D(
            canvas.Scene.GameInstance.GraphicsDevice, canvas.Width, canvas.Height, 
            TextureFormat.R8G8B8A8, TextureUsageFlags.Sampler | TextureUsageFlags.ColorTarget);
    }

    public void Render(CommandBuffer buffer, TextBatch batch) 
    {
        batch.Add(TextString, PixelSize, Color.White);
        batch.UploadBufferData(buffer);
        buffer.BeginRenderPass(new ColorAttachmentInfo(Texture, Color.Transparent));
        buffer.BindGraphicsPipeline(GameContext.MSDFPipeline);
        
        batch.Render(buffer, Matrix);

        buffer.EndRenderPass();
    }
}

public class TextCanvas : Canvas
{
    private TextBatch textBatch;
    private Text[] textPositions;
    private int textCount;
    private Font font;
    public Matrix4x4 TransformMatrix;

    public TextCanvas(Scene scene, GraphicsDevice device, int width, int height, Font font) : base(scene, device, width, height)
    {
        textPositions = new Text[16];
        textBatch = new TextBatch(device);
        this.font = font;
        TransformMatrix = Matrix4x4.CreateOrthographicOffCenter(0, width, height, 0, -1, 1);
    }

    public void Add(Text text) 
    {
        if (textCount == textPositions.Length) 
        {
            Array.Resize(ref textPositions, textPositions.Length * 4);
        }
        text.Position.Y = text.Position.Y + text.PixelSize + 8;
        text.Matrix = Matrix4x4.CreateTranslation(text.Position.X, text.Position.Y, 1)
            * TransformMatrix;
        textPositions[textCount] = text;
        textCount++;
    }

    public override void Draw(CommandBuffer buffer, Batch batch)
    {
        textBatch.Start(font);
        for (int i = 0; i < textCount; i++) 
        {
            var text = textPositions[i];
            text.Render(buffer, textBatch);
        }

        for (int i = 0; i < textCount; i++) 
        {
            var text = textPositions[i];
            batch.Add(text.Texture, GameContext.GlobalSampler, Vector2.Zero, Matrix3x2.Identity);
        }
        batch.FlushVertex(buffer);

        buffer.BeginRenderPass(new ColorAttachmentInfo(CanvasTexture, Color.Transparent));
        buffer.BindGraphicsPipeline(GameContext.DefaultPipeline);

        batch.Draw(buffer);
        buffer.EndRenderPass();
    }
    
}

internal struct InstancedText(Vector2[] positions, Matrix4x4 transformMatrix)
{
    public Vector2[] Positions = positions;
    public Matrix4x4 TransformMatrix = transformMatrix;
}