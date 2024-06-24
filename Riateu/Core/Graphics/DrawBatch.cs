using MoonWorks.Graphics;
using MoonWorks.Math.Float;

namespace Riateu.Graphics;

public class DrawBatch
{
    private Batch batch;
    private unsafe PositionTextureColorVertex* vertices;

    internal unsafe void SetVerticesAndBatch(void* vertices, Batch batch)
    {
        this.vertices = (PositionTextureColorVertex*)vertices;
        this.batch = batch;
    }

    /// <inheritdoc/>
    public void Draw(Quad quad, Vector2 position, Color color, Matrix3x2 transform, float layerDepth = 1)
    {
        Draw(quad, position, color, Vector2.One, Vector2.Zero, transform, layerDepth);
    }

    /// <inheritdoc/>
    public void Draw(Quad quad, Vector2 position, Color color, float layerDepth = 1)
    {
        Draw(quad, position, color, Vector2.One, Vector2.Zero, layerDepth);
    }

    /// <inheritdoc/>
    public void Draw(Quad quad, Vector2 position, Color color, Vector2 scale, Matrix3x2 transform, float layerDepth = 1)
    {
        Draw(quad, position, color, scale, Vector2.Zero, transform, layerDepth);
    }

    /// <inheritdoc/>
    public void Draw(Quad quad, Vector2 position, Color color, Vector2 scale, float layerDepth = 1)
    {
        Draw(quad, position, color, scale, Vector2.Zero, layerDepth);
    }

    /// <inheritdoc/>
    public void Draw(Quad quad, Vector2 position, Color color, Vector2 scale, Vector2 origin, Matrix3x2 transform, float layerDepth = 1)
    {
        Draw(quad, position, color, scale, origin, 0, transform, layerDepth);
    }

    /// <inheritdoc/>
    public void Draw(Quad quad, Vector2 position, Color color, Vector2 scale, Vector2 origin, float layerDepth = 1)
    {
        Draw(quad, position, color, scale, origin, 0, Matrix3x2.Identity, layerDepth);
    }

    /// <inheritdoc/>
    public void Draw(Vector2 position, Color color, Vector2 scale, Vector2 origin, float layerDepth = 1)
    {
        Draw(new Quad(batch.UsedBinding.Texture), position, color, scale, origin, 0, Matrix3x2.Identity, layerDepth);
    }

    /// <inheritdoc/>
    public void Draw(Vector2 position, Color color, float layerDepth = 1)
    {
        Draw(new Quad(batch.UsedBinding.Texture), position, color, Vector2.One, Vector2.Zero, layerDepth);
    }

    /// <inheritdoc/>
    public void Draw(Quad quad, Vector2 position, Color color, Vector2 scale, Vector2 origin, float rotation, float layerDepth = 1)
    {
        Draw(quad, position, color, scale, origin, rotation, Matrix3x2.Identity, layerDepth);
    }

    /// <inheritdoc/>
    public unsafe void Draw(Quad quad, Vector2 position, Color color, Vector2 scale, Vector2 origin, float rotation, Matrix3x2 transform, float layerDepth = 1)
    {
        if (batch.VertexIndex == batch.CurrentMaxTexture)
        {
            batch.ResizeBuffer();
            return;
        }

        float width = quad.Source.W * scale.X;
        float height = quad.Source.H * scale.Y;

        transform = Matrix3x2.CreateTranslation(-origin.X, -origin.Y)
            * Matrix3x2.CreateRotation(rotation)
            * transform;

        var topLeft = new Vector2(position.X, position.Y);
        var topRight = new Vector2(position.X + width, position.Y);
        var bottomLeft = new Vector2(position.X, position.Y + height);
        var bottomRight = new Vector2(position.X + width, position.Y + height);

        var vertexOffset = batch.VertexIndex * 4;

        vertices[vertexOffset].Position = new Vector3(Vector2.Transform(topLeft, transform), layerDepth);
        vertices[vertexOffset + 1].Position = new Vector3(Vector2.Transform(bottomLeft, transform), layerDepth);
        vertices[vertexOffset + 2].Position = new Vector3(Vector2.Transform(topRight, transform), layerDepth);
        vertices[vertexOffset + 3].Position = new Vector3(Vector2.Transform(bottomRight, transform), layerDepth);

        vertices[vertexOffset].Color = color;
        vertices[vertexOffset + 1].Color = color;
        vertices[vertexOffset + 2].Color = color;
        vertices[vertexOffset + 3].Color = color;

        vertices[vertexOffset].TexCoord = quad.UV.TopLeft;
        vertices[vertexOffset + 1].TexCoord = quad.UV.BottomLeft;
        vertices[vertexOffset + 2].TexCoord = quad.UV.TopRight;
        vertices[vertexOffset + 3].TexCoord = quad.UV.BottomRight;

        batch.VertexIndex++;
    }
}
