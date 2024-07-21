using MoonWorks.Graphics;

namespace Riateu.Graphics;

public struct BackbufferTarget 
{
    private Texture texture;

    public BackbufferTarget(Texture texture) 
    {
        this.texture = texture;
    }

    public RenderPass BeginRendering(Color clearColor) 
    {
        return GraphicsExecutor.Executor.BeginRenderPass(new ColorAttachmentInfo(texture, true, clearColor));
    }

    public void EndRendering(RenderPass renderPass) 
    {
        GraphicsExecutor.Executor.EndRenderPass(renderPass);
    }
}