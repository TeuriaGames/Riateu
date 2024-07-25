namespace Riateu.Graphics;

public class Material 
{
    private GraphicsPipeline shaderPipeline;
    public GraphicsPipeline ShaderPipeline => shaderPipeline;

    public GraphicsDevice GraphicsDevice { get; internal set; }

    public Material(GraphicsDevice device, GraphicsPipeline shader) 
    {
        shaderPipeline = shader;
        GraphicsDevice = device;
    }


    public virtual void BindUniforms(VertexUniformBinder uniformBinder) {}
}

public struct VertexUniformBinder 
{
    private uint slotManaged = 1;

    public VertexUniformBinder() {}

    public void BindVertex<T>(GraphicsDevice device, T uniform) 
    where T : unmanaged
    {
        device.DeviceCommandBuffer().PushVertexUniformData<T>(uniform, slotManaged);
    }

    public void BindFragment<T>(GraphicsDevice device, T uniform) 
    where T : unmanaged
    {
        device.DeviceCommandBuffer().PushFragmentUniformData<T>(uniform, slotManaged);
    }
}