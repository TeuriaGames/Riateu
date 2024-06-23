using MoonWorks.Graphics;
using MoonWorks.Math.Float;

namespace Riateu.Graphics;

/// <summary>
/// An interface use to implement a batching system.
/// </summary>
public interface IBatch
{
    /// <summary>
    /// A start of a batch. This should resets all batch state.
    /// </summary>
    void Begin();

    /// <summary>
    /// Adds a vertex data to a batch
    /// </summary>
    /// <param name="baseTexture">A texture to be used for a vertex</param>
    /// <param name="sampler">A sampler to be used for a texture</param>
    /// <param name="position">A position offset that will multiply in a matrix</param>
    /// <param name="color">A color of a drawn texture</param>
    /// <param name="transform">A transform matrix</param>
    /// <param name="layerDepth">A z-depth buffer of a vertex</param>
    void Add(Texture baseTexture, Sampler sampler, Vector2 position, Color color, Matrix3x2 transform, float layerDepth = 1);

    /// <summary>
    /// Adds a vertex data to a batch
    /// </summary>
    /// <param name="baseTexture">A texture to be used for a vertex</param>
    /// <param name="sampler">A sampler to be used for a texture</param>
    /// <param name="position">A position offset that will multiply in a matrix</param>
    /// <param name="color">A color of a drawn texture</param>
    /// <param name="layerDepth">A z-depth buffer of a vertex</param>
    void Add(Texture baseTexture, Sampler sampler, Vector2 position, Color color, float layerDepth = 1);

    /// <summary>
    /// Adds a vertex data to a batch
    /// </summary>
    /// <param name="quad">A <see cref="Riateu.Graphics.Quad"/> to set a quad and coords for the texture</param>
    /// <param name="baseTexture">A texture to be used for a vertex</param>
    /// <param name="sampler">A sampler to be used for a texture</param>
    /// <param name="position">A position offset that will multiply in a matrix</param>
    /// <param name="color">A color of a drawn texture</param>
    /// <param name="transform">A transform matrix</param>
    /// <param name="layerDepth">A z-depth buffer of a vertex</param>
    void Add(Quad quad, Texture baseTexture, Sampler sampler, Vector2 position, Color color, Matrix3x2 transform, float layerDepth = 1);

    /// <summary>
    /// Adds a vertex data to a batch
    /// </summary>
    /// <param name="quad">A <see cref="Riateu.Graphics.Quad"/> to set a quad and coords for the texture</param>
    /// <param name="baseTexture">A texture to be used for a vertex</param>
    /// <param name="sampler">A sampler to be used for a texture</param>
    /// <param name="position">A position offset that will multiply in a matrix</param>
    /// <param name="color">A color of a drawn texture</param>
    /// <param name="layerDepth">A z-depth buffer of a vertex</param>
    void Add(Quad quad, Texture baseTexture, Sampler sampler, Vector2 position, Color color, float layerDepth = 1);


    /// <summary>
    /// Adds a vertex data to a batch
    /// </summary>
    /// <param name="baseTexture">A texture to be used for a vertex</param>
    /// <param name="sampler">A sampler to be used for a texture</param>
    /// <param name="position">A position offset that will multiply in a matrix</param>
    /// <param name="color">A color of a drawn texture</param>
    /// <param name="scale">A scale of a drawn texture</param>
    /// <param name="transform">A transform matrix</param>
    /// <param name="layerDepth">A z-depth buffer of a vertex</param>
    void Add(Texture baseTexture, Sampler sampler, Vector2 position, Color color, Vector2 scale, Matrix3x2 transform, float layerDepth = 1);

    /// <summary>
    /// Adds a vertex data to a batch
    /// </summary>
    /// <param name="baseTexture">A texture to be used for a vertex</param>
    /// <param name="sampler">A sampler to be used for a texture</param>
    /// <param name="position">A position offset that will multiply in a matrix</param>
    /// <param name="color">A color of a drawn texture</param>
    /// <param name="scale">A scale of a drawn texture</param>
    /// <param name="layerDepth">A z-depth buffer of a vertex</param>
    void Add(Texture baseTexture, Sampler sampler, Vector2 position, Color color, Vector2 scale, float layerDepth = 1);

    /// <summary>
    /// Adds a vertex data to a batch
    /// </summary>
    /// <param name="quad">A <see cref="Riateu.Graphics.Quad"/> to set a quad and coords for the texture</param>
    /// <param name="baseTexture">A texture to be used for a vertex</param>
    /// <param name="sampler">A sampler to be used for a texture</param>
    /// <param name="position">A position offset that will multiply in a matrix</param>
    /// <param name="color">A color of a drawn texture</param>
    /// <param name="scale">A scale of a drawn texture</param>
    /// <param name="transform">A transform matrix</param>
    /// <param name="layerDepth">A z-depth buffer of a vertex</param>
    void Add(Quad quad, Texture baseTexture, Sampler sampler, Vector2 position, Color color, Vector2 scale, Matrix3x2 transform, float layerDepth = 1);

    /// <summary>
    /// Adds a vertex data to a batch
    /// </summary>
    /// <param name="quad">A <see cref="Riateu.Graphics.Quad"/> to set a quad and coords for the texture</param>
    /// <param name="baseTexture">A texture to be used for a vertex</param>
    /// <param name="sampler">A sampler to be used for a texture</param>
    /// <param name="position">A position offset that will multiply in a matrix</param>
    /// <param name="color">A color of a drawn texture</param>
    /// <param name="scale">A scale of a drawn texture</param>
    /// <param name="layerDepth">A z-depth buffer of a vertex</param>
    void Add(Quad quad, Texture baseTexture, Sampler sampler, Vector2 position, Color color, Vector2 scale, float layerDepth = 1);

    /// <summary>
    /// Adds a vertex data to a batch
    /// </summary>
    /// <param name="baseTexture">A texture to be used for a vertex</param>
    /// <param name="sampler">A sampler to be used for a texture</param>
    /// <param name="position">A position offset that will multiply in a matrix</param>
    /// <param name="color">A color of a drawn texture</param>
    /// <param name="scale">A scale of a drawn texture</param>
    /// <param name="origin">An origin of a drawn texture</param>
    /// <param name="transform">A transform matrix</param>
    /// <param name="layerDepth">A z-depth buffer of a vertex</param>
    void Add(Texture baseTexture, Sampler sampler, Vector2 position, Color color, Vector2 scale, Vector2 origin, Matrix3x2 transform, float layerDepth = 1);

    /// <summary>
    /// Adds a vertex data to a batch
    /// </summary>
    /// <param name="baseTexture">A texture to be used for a vertex</param>
    /// <param name="sampler">A sampler to be used for a texture</param>
    /// <param name="position">A position offset that will multiply in a matrix</param>
    /// <param name="color">A color of a drawn texture</param>
    /// <param name="scale">A scale of a drawn texture</param>
    /// <param name="origin">An origin of a drawn texture</param>
    /// <param name="layerDepth">A z-depth buffer of a vertex</param>
    void Add(Texture baseTexture, Sampler sampler, Vector2 position, Color color, Vector2 scale, Vector2 origin, float layerDepth = 1);

    /// <summary>
    /// Adds a vertex data to a batch
    /// </summary>
    /// <param name="quad">A <see cref="Riateu.Graphics.Quad"/> to set a quad and coords for the texture</param>
    /// <param name="baseTexture">A texture to be used for a vertex</param>
    /// <param name="sampler">A sampler to be used for a texture</param>
    /// <param name="position">A position offset that will multiply in a matrix</param>
    /// <param name="color">A color of a drawn texture</param>
    /// <param name="scale">A scale of a drawn texture</param>
    /// <param name="origin">An origin of a drawn texture</param>
    /// <param name="transform">A transform matrix</param>
    /// <param name="layerDepth">A z-depth buffer of a vertex</param>
    void Add(Quad quad, Texture baseTexture, Sampler sampler, Vector2 position, Color color, Vector2 scale, Vector2 origin, Matrix3x2 transform, float layerDepth = 1);

    /// <summary>
    /// Adds a vertex data to a batch
    /// </summary>
    /// <param name="quad">A <see cref="Riateu.Graphics.Quad"/> to set a quad and coords for the texture</param>
    /// <param name="baseTexture">A texture to be used for a vertex</param>
    /// <param name="sampler">A sampler to be used for a texture</param>
    /// <param name="position">A position offset that will multiply in a matrix</param>
    /// <param name="color">A color of a drawn texture</param>
    /// <param name="scale">A scale of a drawn texture</param>
    /// <param name="origin">An origin of a drawn texture</param>
    /// <param name="layerDepth">A z-depth buffer of a vertex</param>
    void Add(Quad quad, Texture baseTexture, Sampler sampler, Vector2 position, Color color, Vector2 scale, Vector2 origin, float layerDepth = 1);

    /// <summary>
    /// Adds a vertex data to a batch
    /// </summary>
    /// <param name="quad">A <see cref="Riateu.Graphics.Quad"/> to set a quad and coords for the texture</param>
    /// <param name="baseTexture">A texture to be used for a vertex</param>
    /// <param name="sampler">A sampler to be used for a texture</param>
    /// <param name="position">A position offset that will multiply in a matrix</param>
    /// <param name="color">A color of a drawn texture</param>
    /// <param name="scale">A scale of a drawn texture</param>
    /// <param name="origin">An origin of a drawn texture</param>
    /// <param name="rotation">A rotation of a drawn texture</param>
    /// <param name="transform">A transform matrix</param>
    /// <param name="layerDepth">A z-depth buffer of a vertex</param>
    void Add(Quad quad, Texture baseTexture, Sampler sampler, Vector2 position, Color color, Vector2 scale, Vector2 origin, float rotation, Matrix3x2 transform, float layerDepth = 1);

    /// <summary>
    /// Adds a vertex data to a batch
    /// </summary>
    /// <param name="quad">A <see cref="Riateu.Graphics.Quad"/> to set a quad and coords for the texture</param>
    /// <param name="baseTexture">A texture to be used for a vertex</param>
    /// <param name="sampler">A sampler to be used for a texture</param>
    /// <param name="position">A position offset that will multiply in a matrix</param>
    /// <param name="color">A color of a drawn texture</param>
    /// <param name="scale">A scale of a drawn texture</param>
    /// <param name="origin">An origin of a drawn texture</param>
    /// <param name="rotation">A rotation of a drawn texture</param>
    /// <param name="layerDepth">A z-depth buffer of a vertex</param>
    void Add(Quad quad, Texture baseTexture, Sampler sampler, Vector2 position, Color color, Vector2 scale, Vector2 origin, float rotation, float layerDepth = 1);

    void BindPipeline(GraphicsPipeline pipeline);

    /// <summary>
    /// Push a projection matrix to the batch. Call this before adding any vertices.
    /// </summary>
    /// <param name="matrix">A 4x4 matrix to project to a screen</param>
    void PushMatrix(in Matrix4x4 matrix);

    /// <summary>
    /// Push a camera projection to the batch. Call this before adding any vertices.
    /// </summary>
    /// <param name="camera">A camera to project to a screen</param>
    void PushMatrix(in Camera camera)
    {
        PushMatrix(camera.Transform);
    }

    /// <summary>
    /// Use the default or a previous pushed matrix. Call this after adding any vertices
    /// from a pushed matrix.
    /// </summary>
    void PopMatrix();
    /// <summary>
    /// Draw all vertices into the screen.
    /// </summary>
    /// <param name="renderPass">
    /// A <see cref="MoonWorks.Graphics.RenderPass"/> to bind
    /// all of the buffers and uniforms.
    /// </param>
    void Draw(RenderPass renderPass);
    /// <summary>
    /// Draw all vertices into the screen with a custom view projection.
    /// </summary>
    /// <param name="renderPass">
    /// A <see cref="MoonWorks.Graphics.RenderPass"/> to bind
    /// all of the buffers and uniforms.
    /// </param>
    /// <param name="viewProjection">A 4x4 matrix to project on screen</param>
    void Draw(RenderPass renderPass, Matrix4x4 viewProjection);

    /// <summary>
    /// End of the vertex state and flush all of the vertices. Sent the vertex buffer to the GPU.
    /// </summary>
    /// <param name="cmdBuf">
    /// A <see cref="MoonWorks.Graphics.CommandBuffer"/> for sending the vertex buffer to the GPU
    /// </param>
    void End(CommandBuffer cmdBuf);
}
