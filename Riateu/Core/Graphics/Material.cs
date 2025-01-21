using System;

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


    public virtual void BindUniforms(UniformBinder uniformBinder) {}
}

public struct GraphicsPipelineBuilder 
{
    private Shader vertexShader;
    private Shader fragmentShader;
    private WeakList<(VertexBufferDescription, VertexAttribute[])> inputStates;
    private uint inputTotalIDs;
    private uint attribStrides;

    private DepthStencilState depthStencilState = DepthStencilState.Disable;
    private MultisampleState multiSampleState = MultisampleState.None;
    private RasterizerState rasterizerState = RasterizerState.CCW_CullNone;
    private PrimitiveType primitiveType = PrimitiveType.TriangleList;
    private GraphicsPipelineAttachmentInfo attachmentInfo;
    private BlendConstants blendConstants;

    public GraphicsPipelineBuilder(Shader vertexShader, Shader fragmentShader) 
    {
        this.vertexShader = vertexShader;
        this.fragmentShader = fragmentShader;
        inputStates = new WeakList<(VertexBufferDescription, VertexAttribute[])>();
    }

    public GraphicsPipelineBuilder SetBlendConstants(int r, int g, int b, int a) 
    {
        return SetBlendConstants(new BlendConstants(r, g, b, a));
    }

    public GraphicsPipelineBuilder SetBlendConstants(BlendConstants blendConstants) 
    {
        this.blendConstants = blendConstants;
        return this;
    }

    public GraphicsPipelineBuilder SetAttachmentInfo(GraphicsPipelineAttachmentInfo attachmentInfo) 
    {
        this.attachmentInfo = attachmentInfo;
        return this;
    }

    public GraphicsPipelineBuilder SetDepthStenctilState(DepthStencilState depthStencilState) 
    {
        this.depthStencilState = depthStencilState;
        return this;
    }

    public GraphicsPipelineBuilder SetMultisampleState(MultisampleState multiSampleState) 
    {
        this.multiSampleState = multiSampleState;
        return this;
    }

    public GraphicsPipelineBuilder SetRasterizerState(RasterizerState rasterizerState) 
    {
        this.rasterizerState = rasterizerState;
        return this;
    }

    public GraphicsPipelineBuilder SetPrimitiveType(PrimitiveType primitiveType) 
    {
        this.primitiveType = primitiveType;
        return this;
    }

    public GraphicsPipelineBuilder AddVertexInputState<T>(VertexInputRate inputRate = VertexInputRate.Vertex, uint stepRate = 1) 
    where T : unmanaged, IVertexFormat
    {
        VertexAttribute[] attributes = T.Attributes(inputTotalIDs);
        inputStates.Add((VertexBufferDescription.Create<T>(inputTotalIDs, inputRate, stepRate), attributes));
        attribStrides += (uint)attributes.Length;
        inputTotalIDs++;
        return this;
    }

    public GraphicsPipeline Build(GraphicsDevice device) 
    {
        VertexInputState vertexInputState;
        if (inputTotalIDs > 0)
        {
            VertexBufferDescription[] bindings = new VertexBufferDescription[inputTotalIDs];
            VertexAttribute[] attributes = new VertexAttribute[attribStrides];

            int i = 0;
            int stride = 0;
            foreach (var el in inputStates) 
            {
                bindings[i] = el.Item1;
                ReadOnlySpan<VertexAttribute> attribs = el.Item2.AsSpan();
                for (int j = 0; j < attribs.Length; j++) 
                {
                    attributes[stride++] = attribs[j];
                }
                i++;
            }

            vertexInputState = new VertexInputState(bindings, attributes);
        }
        else 
        {
            vertexInputState = VertexInputState.Empty;
        }


        return new GraphicsPipeline(device, new GraphicsPipelineCreateInfo 
        {
            AttachmentInfo = attachmentInfo,
            DepthStencilState = depthStencilState,
            MultisampleState = multiSampleState,
            PrimitiveType = primitiveType,
            RasterizerState = rasterizerState,
            VertexShader = vertexShader,
            FragmentShader = fragmentShader,
            VertexInputState = vertexInputState,
            BlendConstants = blendConstants
        });
    }
}

public struct UniformBinder()
{
    public void BindVertex<T>(GraphicsDevice device, T uniform, uint slot) 
    where T : unmanaged
    {
        device.DeviceCommandBuffer().PushVertexUniformData<T>(uniform, slot);
    }

    public void BindFragment<T>(GraphicsDevice device, T uniform, uint slot) 
    where T : unmanaged
    {
        device.DeviceCommandBuffer().PushFragmentUniformData<T>(uniform, slot);
    }
}