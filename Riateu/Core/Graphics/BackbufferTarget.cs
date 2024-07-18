using MoonWorks.Graphics;

namespace Riateu.Graphics;

public struct BackbufferTarget 
{
    private Texture texture;
    private RenderPass renderPass;

    public BackbufferTarget(Texture texture) 
    {
        this.texture = texture;
    }

    public void BeginRendering(Color clearColor) 
    {
        renderPass = GraphicsExecutor.Executor.BeginRenderPass(new ColorAttachmentInfo(texture, true, clearColor));
    }

    public void Render(IRenderable renderable) 
    {
        renderable.Render(renderPass);
    }

    public void EndRendering() 
    {
        GraphicsExecutor.Executor.EndRenderPass(renderPass);
    }
}