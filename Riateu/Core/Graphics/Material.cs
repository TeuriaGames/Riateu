using MoonWorks.Graphics;

namespace Riateu.Graphics;

public class Material 
{
    private GraphicsPipeline shaderPipeline;
    public GraphicsPipeline ShaderPipeline => shaderPipeline;

    public Material(GraphicsPipeline shader) 
    {
        shaderPipeline = shader;
    }


    public virtual void BindUniforms(VertexUniformBinder uniformBinder) {}
}

public struct VertexUniformBinder 
{
    private uint slotManaged = 1;

    public VertexUniformBinder() {}

    public void BindVertex<T>(T uniform) 
    where T : unmanaged
    {
        GraphicsExecutor.Executor.PushVertexUniformData<T>(uniform, slotManaged++);
    }

    public void BindFragment<T>(T uniform) 
    where T : unmanaged
    {
        GraphicsExecutor.Executor.PushVertexUniformData<T>(uniform, slotManaged++);
    }
}