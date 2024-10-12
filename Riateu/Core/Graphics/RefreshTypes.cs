using System;
using System.Runtime.InteropServices;
using SDL3;

namespace Riateu.Graphics;

#region Enums
public enum PrimitiveType
{
    PointList,
    LineList,
    LineStrip,
    TriangleList,
    TriangleStrip
}

public enum LoadOp
{
    Load,
    Clear,
    DontCare
}

public enum StoreOp
{
    Store,
    DontCare
}

public enum IndexElementSize
{
    SixteenBit,
    ThirtyTwoBit
}

public enum TextureFormat
{
    Invalid = -1,

    /* Unsigned Normalized Float Color Formats */
    R8G8B8A8,
    B8G8R8A8,
    B5G6R5,
    B5G5R5A1,
    B4G4R4A4,
    R10G10B10A2,
    R16G16,
    R16G16B16A16,
    R8,
    A8,
    /* Compressed Unsigned Normalized Float Color Formats */
    BC1,
    BC2,
    BC3,
    BC7,
    /* Signed Normalized Float Color Formats  */
    R8G8_SNORM,
    R8G8B8A8_SNORM,
    /* Signed Float Color Formats */
    R16_SFLOAT,
    R16G16_SFLOAT,
    R16G16B16A16_SFLOAT,
    R32_SFLOAT,
    R32G32_SFLOAT,
    R32G32B32A32_SFLOAT,
    /* Unsigned Integer Color Formats */
    R8_UINT,
    R8G8_UINT,
    R8G8B8A8_UINT,
    R16_UINT,
    R16G16_UINT,
    R16G16B16A16_UINT,
    /* SRGB Color Formats */
    R8G8B8A8_SRGB,
    B8G8R8A8_SRGB,
    /* Compressed SRGB Color Formats */
    BC3_SRGB,
    BC7_SRGB,
    /* Depth Formats */
    D16_UNORM,
    D24_UNORM,
    D32_SFLOAT,
    D24_UNORM_S8_UINT,
    D32_SFLOAT_S8_UINT
}

[Flags]
public enum TextureUsageFlags
{
    Sampler = 0x1,
    ColorTarget = 0x2,
    DepthStencil = 0x4,
    GraphicsStorage = 0x8,
    ComputeStorageRead = 0x20,
    ComputeStorageWrite = 0x40
}

public enum SampleCount
{
    One,
    Two,
    Four,
    Eight
}

public enum CubeMapFace
{
    PositiveX,
    NegativeX,
    PositiveY,
    NegativeY,
    PositiveZ,
    NegativeZ
}

[Flags]
public enum BufferUsageFlags
{
    Vertex = 0x1,
    Index = 0x2,
    Indirect = 0x4,
    GraphicsStorage = 0x8,
    ComputeStorageRead = 0x20,
    ComputeStorageWrite = 0x40
}

public enum TransferBufferUsage
{
    Upload,
    Download
}

public enum ShaderStage
{
    Vertex,
    Fragment
}

public enum ShaderFormat
{
    Private = 0x1, 
    SPIRV = 0x2,
    DXBC = 0x4,
    DXIL = 0x08,
    MSL = 0x10,
    METALLIB = 0x20
}

public enum VertexElementFormat
{
    Uint,
    Float,
    Vector2,
    Vector3,
    Vector4,
    Color,
    Byte4,
    Short2,
    Short4,
    NormalizedShort2,
    NormalizedShort4,
    HalfVector2,
    HalfVector4
}

public enum VertexInputRate
{
    Vertex,
    Instance
}

public enum FillMode
{
    Fill,
    Line
}

public enum CullMode
{
    None,
    Front,
    Back
}

public enum FrontFace
{
    CounterClockwise,
    Clockwise
}

public enum CompareOp
{
    Never,
    Less,
    Equal,
    LessOrEqual,
    Greater,
    NotEqual,
    GreaterOrEqual,
    Always
}

public enum StencilOp
{
    Keep,
    Zero,
    Replace,
    IncrementAndClamp,
    DecrementAndClamp,
    Invert,
    IncrementAndWrap,
    DecrementAndWrap
}

public enum BlendOp
{
    Add,
    Subtract,
    ReverseSubtract,
    Min,
    Max
}

public enum BlendFactor
{
    Zero,
    One,
    SourceColor,
    OneMinusSourceColor,
    DestinationColor,
    OneMinusDestinationColor,
    SourceAlpha,
    OneMinusSourceAlpha,
    DestinationAlpha,
    OneMinusDestinationAlpha,
    ConstantColor,
    OneMinusConstantColor,
    SourceAlphaSaturate
}

[Flags]
public enum ColorComponentFlags
{
    None = 0x0,
    R = 0x1,
    G = 0x2,
    B = 0x4,
    A = 0x8,
    RGB = R | G | B,
    RGBA = R | G | B | A
}

public enum Filter
{
    Nearest,
    Linear
}

public enum SamplerMipmapMode
{
    Nearest,
    Linear
}

public enum SamplerAddressMode
{
    Repeat,
    MirroredRepeat,
    ClampToEdge
}

public enum PresentMode
{
    VSync,
    Immediate,
    Mailbox
}

public enum SwapchainComposition
{
    SDR,
    SDRLinear,
    HDRExtendedLinear,
    HDR10_ST2084
}

[Flags]
public enum BackendFlags
{
    Invalid = 0x0,
    Vulkan = 0x1,
    D3D11 = 0x2,
    Metal = 0x4,
    All = Vulkan | D3D11 | Metal
}

public enum TextureType
{
    Texture2D = 0,
    Texture2DArray = 1,
    Texture3D = 2,
    Cube = 3
}

#endregion


#region Structs
public struct Viewport(float Width, float Height)
{
    public float X;
    public float Y;
    public float Width = Width;
    public float Height = Height;
    public float MinDepth;
    public float MaxDepth;


    public SDL.SDL_GPUViewport ToSDLGpu() 
    {
        return new SDL.SDL_GPUViewport() 
        {
            x = X,
            y = Y,
            w = Width,
            h = Height,
            min_depth = MinDepth,
            max_depth = MaxDepth
        };
    }
}

public struct TextureCreateInfo
{
    public uint Width;
    public uint Height;
    public uint Depth;
    public TextureType TextureType;
    public uint LayerCount;
    public uint LevelCount;
    public SampleCount SampleCount;
    public TextureFormat Format;
    public TextureUsageFlags UsageFlags;

    public SDL.SDL_GPUTextureCreateInfo ToSDLGpu() 
    {
        return new SDL.SDL_GPUTextureCreateInfo() 
        {
            type = (SDL.SDL_GPUTextureType)TextureType,
            format = (SDL.SDL_GPUTextureFormat)Format,
            width = Width,
            height = Height,
            layer_count_or_depth = LayerCount,
            num_levels = LayerCount,
            sample_count = (SDL.SDL_GPUSampleCount)SampleCount,
            usage = (SDL.SDL_GPUTextureUsageFlags)UsageFlags
        };
    }
}

public struct ColorAttachmentInfo(Texture texture, Color clearColor, bool cycle, LoadOp loadOp = LoadOp.Clear, StoreOp storeOp = StoreOp.Store)
{
    public Texture TextureSlice = texture;
    public Color ClearColor = clearColor;
    public LoadOp LoadOp = loadOp;
    public StoreOp StoreOp = storeOp;
    public bool Cycle = cycle; 

    public SDL.SDL_GPUColorTargetInfo ToSDLGpu() 
    {
        return new SDL.SDL_GPUColorTargetInfo() 
        {
            texture = TextureSlice.Handle,
            clear_color = ClearColor.ToSDLGpu(),
            load_op = (SDL.SDL_GPULoadOp)LoadOp,
            store_op = (SDL.SDL_GPUStoreOp)StoreOp,
            cycle = Cycle
        };
    }
}

public struct DepthStencilAttachmentInfo(
    Texture texture, 
    float depth,
    byte stencil,
    bool cycle,
    LoadOp loadOp = LoadOp.DontCare, 
    StoreOp storeOp = StoreOp.DontCare,
    LoadOp stencilLoadOp = LoadOp.DontCare,
    StoreOp stencilStoreOp = StoreOp.DontCare
)
{
    public Texture TextureSlice = texture;
    public float Depth = depth;
    public LoadOp LoadOp = loadOp;
    public StoreOp StoreOp = storeOp;
    public LoadOp StencilLoadOp = stencilLoadOp;
    public StoreOp StencilStoreOp = stencilStoreOp;
    public byte Stencil = stencil;
    public bool Cycle = cycle;

    public SDL.SDL_GPUDepthStencilTargetInfo ToSDLGpu() 
    {
        return new SDL.SDL_GPUDepthStencilTargetInfo() 
        {
            texture = TextureSlice.Handle,
            clear_depth = Depth,
            clear_stencil = Stencil,
            load_op = (SDL.SDL_GPULoadOp)LoadOp,
            store_op = (SDL.SDL_GPUStoreOp)StoreOp,
            stencil_load_op = (SDL.SDL_GPULoadOp)StencilLoadOp,
            stencil_store_op = (SDL.SDL_GPUStoreOp)StencilLoadOp,
            cycle = Cycle
        };
    }
}

public struct StorageBufferReadWriteBinding(RawBuffer buffer, bool cycle)
{
    public RawBuffer Buffer = buffer;
    public bool Cycle = cycle; 

    public SDL.SDL_GPUStorageBufferReadWriteBinding ToSDLGpu() 
    {
        return new SDL.SDL_GPUStorageBufferReadWriteBinding() 
        {
            buffer = Buffer.Handle,
            cycle = Cycle
        };
    }
}

public struct StorageTextureReadWriteBinding(Texture texture, bool cycle)
{
    public Texture TextureSlice = texture;
    public bool Cycle = cycle;

    public SDL.SDL_GPUStorageTextureReadWriteBinding ToSDLGpu() 
    {
        return new SDL.SDL_GPUStorageTextureReadWriteBinding() 
        {
            texture = TextureSlice.Handle,
            cycle = Cycle
        };
    }
}

public unsafe struct ComputePipelineCreateInfo
{
    public ShaderFormat ShaderFormat;
    public uint ReadOnlyStorageTextureCount;
    public uint ReadOnlyStorageBufferCount;
    public uint ReadWriteStorageTextureCount;
    public uint ReadWriteStorageBufferCount;
    public uint UniformBufferCount;
    public uint SamplersCount;
    public uint ThreadCountX;
    public uint ThreadCountY;
    public uint ThreadCountZ;
}

public struct TextureLocation(Texture slice, uint x, uint y, uint z)
{
    public Texture TextureSlice = slice;
    public uint X = x;
    public uint Y = y;
    public uint Z = z;

    public SDL.SDL_GPUTextureLocation ToSDLGpu() 
    {
        return new SDL.SDL_GPUTextureLocation() 
        {
            texture = TextureSlice.Handle,
            x = X,
            y = Y,
            z = Z
        };
    }
}

public struct BlitRegion
{
    public Texture Texture;
    public uint MipLevel;
    public uint LayerDepthOrPlane;
    public uint X;
    public uint Y;
    public uint W;
    public uint H;

    public BlitRegion(Texture texture, uint x, uint y, uint mipLevel, uint width, uint height, uint depthOrPlane)  
    {
        Texture = texture;
        X = x;
        Y = y;
        W = width;
        H = height;
        MipLevel = mipLevel;
        LayerDepthOrPlane = depthOrPlane;
    }

    public BlitRegion(Texture texture)  
    {
        Texture = texture;
        X = 0;
        Y = 0;
        W = texture.Width;
        H = texture.Height;
        MipLevel = 1;
        LayerDepthOrPlane = texture.Depth;
    }

    public SDL.SDL_GPUBlitRegion ToSDLGpu() 
    {
        return new SDL.SDL_GPUBlitRegion() 
        {
            texture = Texture.Handle,
            x = X,
            y = Y,
            w = W,
            h = H,
            mip_level = MipLevel,
            layer_or_depth_plane = LayerDepthOrPlane
        };
    }
}

public struct TextureRegion
{
    public Texture Texture;
    public uint X;
    public uint Y;
    public uint Z;
    public uint W;
    public uint H;
    public uint D;

    public TextureRegion(Texture texture, uint x, uint y, uint z, uint width, uint height, uint depth)  
    {
        Texture = texture;
        X = x;
        Y = y;
        Z = z;
        W = width;
        H = height;
        D = depth;
    }

    public TextureRegion(Texture texture)  
    {
        Texture = texture;
        X = 0;
        Y = 0;
        Z = 0;
        W = texture.Width;
        H = texture.Height;
        D = texture.Depth;
    }

    public SDL.SDL_GPUTextureRegion ToSDLGpu() 
    {
        return new SDL.SDL_GPUTextureRegion() 
        {
            texture = Texture.Handle,
            x = X,
            y = Y,
            z = Z,
            w = W,
            h = H,
            d = D
        };
    }
}

public struct TransferBufferLocation(TransferBuffer buffer, uint offset = 0)
{
    public TransferBuffer TransferBuffer = buffer;
    public uint Offset = offset;

    public SDL.SDL_GPUTransferBufferLocation ToSDLGpu() 
    {
        return new SDL.SDL_GPUTransferBufferLocation() 
        {
            transfer_buffer = TransferBuffer.Handle,
            offset = Offset
        };
    }
}

public struct BufferRegion(RawBuffer buffer, uint offset, uint size)
{
    public RawBuffer Buffer = buffer;
    public uint Offset = offset;
    public uint Size = size;

    public SDL.SDL_GPUBufferRegion ToSDLGpu() 
    {
        return new SDL.SDL_GPUBufferRegion() 
        {
            buffer = Buffer.Handle,
            offset = Offset,
            size = Size
        };
    }
}

public struct BufferLocation(RawBuffer buffer, uint offset)
{
    public RawBuffer Buffer = buffer;
    public uint Offset = offset;

    public SDL.SDL_GPUBufferLocation ToSDLGpu() 
    {
        return new SDL.SDL_GPUBufferLocation() 
        {
            buffer = Buffer.Handle,
            offset = Offset,
        };
    }
}

public struct ShaderCreateInfo
{
	public ShaderStage ShaderStage;
	public ShaderFormat ShaderFormat;
	public uint SamplerCount;
	public uint StorageTextureCount;
	public uint StorageBufferCount;
	public uint UniformBufferCount;

	public static ShaderCreateInfo None = new ShaderCreateInfo();
}

public struct GraphicsPipelineCreateInfo
{
    public Shader VertexShader;
    public Shader FragmentShader;
    public DepthStencilState DepthStencilState;
    public MultisampleState MultisampleState;
    public RasterizerState RasterizerState;
    public PrimitiveType PrimitiveType;
    public VertexInputState VertexInputState;
    public GraphicsPipelineAttachmentInfo AttachmentInfo;
    public BlendConstants BlendConstants;
}

public struct VertexInputState 
{
    public VertexBufferDescription[] VertexBindings;
    public VertexAttribute[] VertexAttributes;

    public static readonly VertexInputState Empty = new VertexInputState() 
    {
        VertexBindings = Array.Empty<VertexBufferDescription>(),
        VertexAttributes = Array.Empty<VertexAttribute>()
    };

    public VertexInputState(VertexBufferDescription vertexBinding, VertexAttribute[] vertexAttributes) 
    {
        VertexBindings = [vertexBinding];
        VertexAttributes = vertexAttributes;
    }

    public VertexInputState(VertexBufferDescription[] vertexBindings, VertexAttribute[] vertexAttributes) 
    {
        VertexBindings = vertexBindings;
        VertexAttributes = vertexAttributes;
    }
}

public struct GraphicsPipelineAttachmentInfo 
{
    public ColorAttachmentDescription[] ColorAttachmentDescriptions;
    public bool HasDepthStencilAttachment;
    public TextureFormat DepthStencilFormat;

    public GraphicsPipelineAttachmentInfo(params ColorAttachmentDescription[] colorAttachmentDescriptions) 
    {
        ColorAttachmentDescriptions = colorAttachmentDescriptions;
        HasDepthStencilAttachment = false;
        DepthStencilFormat = TextureFormat.D16_UNORM;
    }


    public GraphicsPipelineAttachmentInfo(TextureFormat depthStencilFormat, params ColorAttachmentDescription[] colorAttachmentDescriptions) 
    {
        ColorAttachmentDescriptions = colorAttachmentDescriptions;
        HasDepthStencilAttachment = true;
        DepthStencilFormat = depthStencilFormat;
    }
}

public struct ColorAttachmentBlendState 
{
    public bool BlendEnable;
    public BlendOp AlphaBlendOp;
    public BlendOp ColorBlendOp;
    public ColorComponentFlags ColorWriteMask;
    public BlendFactor DestinationAlphaBlendFactor;
    public BlendFactor DestinationColorBlendFactor;
    public BlendFactor SourceAlphaBlendFactor;
    public BlendFactor SourceColorBlendFactor;

	public static readonly ColorAttachmentBlendState Additive = new ColorAttachmentBlendState
	{
		BlendEnable = true,
		AlphaBlendOp = BlendOp.Add,
		ColorBlendOp = BlendOp.Add,
		ColorWriteMask = ColorComponentFlags.RGBA,
		SourceColorBlendFactor = BlendFactor.SourceAlpha,
		SourceAlphaBlendFactor = BlendFactor.SourceAlpha,
		DestinationColorBlendFactor = BlendFactor.One,
		DestinationAlphaBlendFactor = BlendFactor.One
	};

	public static readonly ColorAttachmentBlendState AlphaBlend = new ColorAttachmentBlendState
	{
		BlendEnable = true,
		AlphaBlendOp = BlendOp.Add,
		ColorBlendOp = BlendOp.Add,
		ColorWriteMask = ColorComponentFlags.RGBA,
		SourceColorBlendFactor = BlendFactor.One,
		SourceAlphaBlendFactor = BlendFactor.One,
		DestinationColorBlendFactor = BlendFactor.OneMinusSourceAlpha,
		DestinationAlphaBlendFactor = BlendFactor.OneMinusSourceAlpha
	};

	public static readonly ColorAttachmentBlendState NonPremultiplied = new ColorAttachmentBlendState
	{
		BlendEnable = true,
		AlphaBlendOp = BlendOp.Add,
		ColorBlendOp = BlendOp.Add,
		ColorWriteMask = ColorComponentFlags.RGBA,
		SourceColorBlendFactor = BlendFactor.SourceAlpha,
		SourceAlphaBlendFactor = BlendFactor.SourceAlpha,
		DestinationColorBlendFactor = BlendFactor.OneMinusSourceAlpha,
		DestinationAlphaBlendFactor = BlendFactor.OneMinusSourceAlpha
	};

	public static readonly ColorAttachmentBlendState Opaque = new ColorAttachmentBlendState
	{
		BlendEnable = true,
		AlphaBlendOp = BlendOp.Add,
		ColorBlendOp = BlendOp.Add,
		ColorWriteMask = ColorComponentFlags.RGBA,
		SourceColorBlendFactor = BlendFactor.One,
		SourceAlphaBlendFactor = BlendFactor.One,
		DestinationColorBlendFactor = BlendFactor.Zero,
		DestinationAlphaBlendFactor = BlendFactor.Zero
	};

	public static readonly ColorAttachmentBlendState None = new ColorAttachmentBlendState
	{
		BlendEnable = false,
		ColorWriteMask = ColorComponentFlags.RGBA
	};

	public static readonly ColorAttachmentBlendState Disable = new ColorAttachmentBlendState
	{
		BlendEnable = false,
		ColorWriteMask = ColorComponentFlags.None
	};

	public SDL.SDL_GPUColorTargetBlendState ToSDLGpu()
	{
		return new SDL.SDL_GPUColorTargetBlendState
		{
			enable_blend = BlendEnable,
			alpha_blend_op = (SDL.SDL_GPUBlendOp) AlphaBlendOp,
			color_blend_op = (SDL.SDL_GPUBlendOp) ColorBlendOp,
			color_write_mask = (SDL.SDL_GPUColorComponentFlags) ColorWriteMask,
			dst_alpha_blendfactor = (SDL.SDL_GPUBlendFactor) DestinationAlphaBlendFactor,
			dst_color_blendfactor = (SDL.SDL_GPUBlendFactor) DestinationColorBlendFactor,
			src_alpha_blendfactor = (SDL.SDL_GPUBlendFactor) SourceAlphaBlendFactor,
			src_color_blendfactor = (SDL.SDL_GPUBlendFactor) SourceColorBlendFactor
		};
	}
}

[StructLayout(LayoutKind.Sequential)]
public struct ColorAttachmentDescription(TextureFormat format, ColorAttachmentBlendState blendState) 
{
    public TextureFormat Format = format;
    public ColorAttachmentBlendState BlendState = blendState;
}

public struct RasterizerState 
{
    public CullMode CullMode;
    public float DepthBiasClamp;
    public float DepthBiasConstantFactor;
    public bool DepthBiasEnable;
    public bool DepthClipEnable;
    public float DepthBiasSlopeFactor;
    public FillMode FillMode;
    public FrontFace FrontFace;

	public static readonly RasterizerState CW_CullFront = new RasterizerState
	{
		CullMode = CullMode.Front,
		FrontFace = FrontFace.Clockwise,
		FillMode = FillMode.Fill,
		DepthBiasEnable = false
	};

	public static readonly RasterizerState CW_CullBack = new RasterizerState
	{
		CullMode = CullMode.Back,
		FrontFace = FrontFace.Clockwise,
		FillMode = FillMode.Fill,
		DepthBiasEnable = false
	};

	public static readonly RasterizerState CW_CullNone = new RasterizerState
	{
		CullMode = CullMode.None,
		FrontFace = FrontFace.Clockwise,
		FillMode = FillMode.Fill,
		DepthBiasEnable = false
	};

	public static readonly RasterizerState CW_Wireframe = new RasterizerState
	{
		CullMode = CullMode.None,
		FrontFace = FrontFace.Clockwise,
		FillMode = FillMode.Line,
		DepthBiasEnable = false
	};

	public static readonly RasterizerState CCW_CullFront = new RasterizerState
	{
		CullMode = CullMode.Front,
		FrontFace = FrontFace.CounterClockwise,
		FillMode = FillMode.Fill,
		DepthBiasEnable = false
	};

	public static readonly RasterizerState CCW_CullBack = new RasterizerState
	{
		CullMode = CullMode.Back,
		FrontFace = FrontFace.CounterClockwise,
		FillMode = FillMode.Fill,
		DepthBiasEnable = false
	};

	public static readonly RasterizerState CCW_CullNone = new RasterizerState
	{
		CullMode = CullMode.None,
		FrontFace = FrontFace.CounterClockwise,
		FillMode = FillMode.Fill,
		DepthBiasEnable = false
	};

	public static readonly RasterizerState CCW_Wireframe = new RasterizerState
	{
		CullMode = CullMode.None,
		FrontFace = FrontFace.CounterClockwise,
		FillMode = FillMode.Line,
		DepthBiasEnable = false
	};

	public SDL.SDL_GPURasterizerState ToSDLGpu()
	{
		return new SDL.SDL_GPURasterizerState
		{
			cull_mode = (SDL.SDL_GPUCullMode) CullMode,
			depth_bias_clamp = DepthBiasClamp,
			depth_bias_constant_factor = DepthBiasConstantFactor,
			enable_depth_bias = DepthBiasEnable,
            enable_depth_clip = DepthClipEnable,
            depth_bias_slope_factor = DepthBiasSlopeFactor,
			fill_mode = (SDL.SDL_GPUFillMode) FillMode,
			front_face = (SDL.SDL_GPUFrontFace) FrontFace
		};
	}
}

public struct MultisampleState(SampleCount sampleCount, uint sampleMask, bool enableMask)
{
    public SampleCount MultisampleCount = sampleCount;
    public uint SampleMask = sampleMask;
    public bool MaskEnable = enableMask;

    public static readonly MultisampleState None = new MultisampleState() 
    {
        MultisampleCount = SampleCount.One,
        SampleMask = uint.MaxValue,
        MaskEnable = false
    };

    public SDL.SDL_GPUMultisampleState ToSDLGpu() 
    {
        return new SDL.SDL_GPUMultisampleState() 
        {
            sample_count = (SDL.SDL_GPUSampleCount)MultisampleCount,
            sample_mask = SampleMask,
            enable_mask = MaskEnable
        };
    }
}

public struct BlendConstants 
{
    public float R, G, B, A;
}

public struct VertexAttribute(uint bufferSlot, uint location, VertexElementFormat format, uint offset)
{
    public uint Location = location;
    public uint BufferSlot = bufferSlot;
    public VertexElementFormat Format = format;
    public uint Offset = offset;

    public SDL.SDL_GPUVertexAttribute ToSDLGpu() 
    {
        return new SDL.SDL_GPUVertexAttribute
        {
            location = Location,
            buffer_slot = BufferSlot,
            format = (SDL.SDL_GPUVertexElementFormat)Format,
            offset = Offset
        };
    }
}

public struct VertexBufferDescription
{
    public uint Slot;
    public uint Stride;
    public VertexInputRate InputRate;
    public uint StepRate;

    public unsafe static VertexBufferDescription Create<T>(uint slot, VertexInputRate inputRate = VertexInputRate.Vertex, uint stepRate = 1) 
    where T : unmanaged
    {
        return new VertexBufferDescription 
        {
            Slot = slot,
            InputRate = inputRate,
            StepRate = stepRate,
            Stride = (uint)sizeof(T)
        };
    }

    public SDL.SDL_GPUVertexBufferDescription ToSDLGpu() 
    {
        return new SDL.SDL_GPUVertexBufferDescription() 
        {
            slot = Slot,
            instance_step_rate = StepRate,
            input_rate = (SDL.SDL_GPUVertexInputRate)InputRate,
            pitch = Stride
        };
    }
}

public struct SamplerCreateInfo
{
    public Filter MinFilter;
    public Filter MagFilter;
    public SamplerMipmapMode MipmapMode;
    public SamplerAddressMode AddressModeU;
    public SamplerAddressMode AddressModeV;
    public SamplerAddressMode AddressModeW;
    public float MipLodBias;
    public bool AnisotropyEnable;
    public float MaxAnisotropy;
    public bool CompareEnable;
    public CompareOp CompareOp;
    public float MinLod;
    public float MaxLod;

	public static readonly SamplerCreateInfo AnisotropicClamp = new SamplerCreateInfo
	{
		MinFilter = Filter.Linear,
		MagFilter = Filter.Linear,
		MipmapMode = SamplerMipmapMode.Linear,
		AddressModeU = SamplerAddressMode.ClampToEdge,
		AddressModeV = SamplerAddressMode.ClampToEdge,
		AddressModeW = SamplerAddressMode.ClampToEdge,
		CompareEnable = false,
		AnisotropyEnable = true,
		MaxAnisotropy = 4,
		MipLodBias = 0f,
		MinLod = 0,
		MaxLod = 1000
	};

	public static readonly SamplerCreateInfo AnisotropicWrap = new SamplerCreateInfo
	{
		MinFilter = Filter.Linear,
		MagFilter = Filter.Linear,
		MipmapMode = SamplerMipmapMode.Linear,
		AddressModeU = SamplerAddressMode.Repeat,
		AddressModeV = SamplerAddressMode.Repeat,
		AddressModeW = SamplerAddressMode.Repeat,
		CompareEnable = false,
		AnisotropyEnable = true,
		MaxAnisotropy = 4,
		MipLodBias = 0f,
		MinLod = 0,
		MaxLod = 1000
	};

	public static readonly SamplerCreateInfo LinearClamp = new SamplerCreateInfo
	{
		MinFilter = Filter.Linear,
		MagFilter = Filter.Linear,
		MipmapMode = SamplerMipmapMode.Linear,
		AddressModeU = SamplerAddressMode.ClampToEdge,
		AddressModeV = SamplerAddressMode.ClampToEdge,
		AddressModeW = SamplerAddressMode.ClampToEdge,
		CompareEnable = false,
		AnisotropyEnable = false,
		MipLodBias = 0f,
		MinLod = 0,
		MaxLod = 1000
	};

	public static readonly SamplerCreateInfo LinearWrap = new SamplerCreateInfo
	{
		MinFilter = Filter.Linear,
		MagFilter = Filter.Linear,
		MipmapMode = SamplerMipmapMode.Linear,
		AddressModeU = SamplerAddressMode.Repeat,
		AddressModeV = SamplerAddressMode.Repeat,
		AddressModeW = SamplerAddressMode.Repeat,
		CompareEnable = false,
		AnisotropyEnable = false,
		MipLodBias = 0f,
		MinLod = 0,
		MaxLod = 1000
	};

	public static readonly SamplerCreateInfo PointClamp = new SamplerCreateInfo
	{
		MinFilter = Filter.Nearest,
		MagFilter = Filter.Nearest,
		MipmapMode = SamplerMipmapMode.Nearest,
		AddressModeU = SamplerAddressMode.ClampToEdge,
		AddressModeV = SamplerAddressMode.ClampToEdge,
		AddressModeW = SamplerAddressMode.ClampToEdge,
		CompareEnable = false,
		AnisotropyEnable = false,
		MipLodBias = 0f,
		MinLod = 0,
		MaxLod = 1000
	};

	public static readonly SamplerCreateInfo PointWrap = new SamplerCreateInfo
	{
		MinFilter = Filter.Nearest,
		MagFilter = Filter.Nearest,
		MipmapMode = SamplerMipmapMode.Nearest,
		AddressModeU = SamplerAddressMode.Repeat,
		AddressModeV = SamplerAddressMode.Repeat,
		AddressModeW = SamplerAddressMode.Repeat,
		CompareEnable = false,
		AnisotropyEnable = false,
		MipLodBias = 0f,
		MinLod = 0,
		MaxLod = 1000
	};

    public SDL.SDL_GPUSamplerCreateInfo ToSDLGpu() 
    {
        return new SDL.SDL_GPUSamplerCreateInfo() 
        {
            min_filter = (SDL.SDL_GPUFilter)MinFilter,
            mag_filter = (SDL.SDL_GPUFilter)MagFilter,
            mipmap_mode = (SDL.SDL_GPUSamplerMipmapMode)MipmapMode,
            address_mode_u = (SDL.SDL_GPUSamplerAddressMode)AddressModeU,
            address_mode_v = (SDL.SDL_GPUSamplerAddressMode)AddressModeV,
            address_mode_w = (SDL.SDL_GPUSamplerAddressMode)AddressModeW,
            mip_lod_bias = MipLodBias,
            enable_anisotropy = AnisotropyEnable,
            max_anisotropy = MaxAnisotropy,
            enable_compare = CompareEnable,
            compare_op = (SDL.SDL_GPUCompareOp)CompareOp,
            min_lod = MinLod,
            max_lod = MaxLod
        };
    }
}

public struct StencilOpState 
{
    public StencilOp FailOp;
    public StencilOp PassOp;
    public StencilOp DepthFailOp;
    public CompareOp CompareOp;

    public SDL.SDL_GPUStencilOpState ToSDLGpu() 
    {
        return new SDL.SDL_GPUStencilOpState() 
        {
            fail_op = (SDL.SDL_GPUStencilOp)FailOp,
            pass_op = (SDL.SDL_GPUStencilOp)PassOp,
            depth_fail_op = (SDL.SDL_GPUStencilOp)DepthFailOp,
            compare_op = (SDL.SDL_GPUCompareOp)CompareOp
        };
    }
}

public struct DepthStencilState 
{
    public bool DepthTestEnable;
    public StencilOpState BackStencilState;
    public StencilOpState FrontStencilState;
    public byte CompareMask;
    public byte WriteMask;
    public CompareOp CompareOp;
    public bool DepthWriteEnable;
    public bool StencilTestEnable;

	public static readonly DepthStencilState DepthReadWrite = new DepthStencilState
	{
		DepthTestEnable = true,
		DepthWriteEnable = true,
		StencilTestEnable = false,
		CompareOp = CompareOp.LessOrEqual
	};

	public static readonly DepthStencilState DepthRead = new DepthStencilState
	{
		DepthTestEnable = true,
		DepthWriteEnable = false,
		StencilTestEnable = false,
		CompareOp = CompareOp.LessOrEqual
	};

	public static readonly DepthStencilState Disable = new DepthStencilState
	{
		DepthTestEnable = false,
		DepthWriteEnable = false,
		StencilTestEnable = false
	};

    public SDL.SDL_GPUDepthStencilState ToSDLGpu() 
    {
        return new SDL.SDL_GPUDepthStencilState() 
        {
            enable_depth_test = DepthTestEnable,
            back_stencil_state = BackStencilState.ToSDLGpu(),
            front_stencil_state = FrontStencilState.ToSDLGpu(),
            compare_mask = CompareMask,
            write_mask = WriteMask,
            compare_op = (SDL.SDL_GPUCompareOp)CompareOp,
            enable_depth_write = DepthWriteEnable,
            enable_stencil_test = StencilTestEnable
        };
    }
}

public struct BufferBinding(RawBuffer buffer, uint offset)
{
    public RawBuffer Buffer = buffer;
    public uint Offset = offset;

    public SDL.SDL_GPUBufferBinding ToSDLGpu() 
    {
        return new SDL.SDL_GPUBufferBinding() 
        {
            buffer = Buffer.Handle,
            offset = Offset
        };
    }
}

public struct TextureSamplerBinding(Texture texture, Sampler sampler)
{
    public Texture Texture = texture;
    public Sampler Sampler = sampler;

    public SDL.SDL_GPUTextureSamplerBinding ToSDLGpu() 
    {
        return new SDL.SDL_GPUTextureSamplerBinding() 
        {
            texture = Texture.Handle,
            sampler = Sampler.Handle
        };
    }
}

public struct TextureTransferInfo(TransferBuffer transferBuffer, uint offset = 0, uint imagePitch = 0, uint imageHeight = 0)
{
	public TransferBuffer TransferBuffer = transferBuffer;
	public uint Offset = offset;
	public uint ImagePitch = imagePitch;
	public uint ImageHeight = imageHeight;

	public SDL.SDL_GPUTextureTransferInfo ToSDLGpu()
	{
		return new SDL.SDL_GPUTextureTransferInfo
		{
			transfer_buffer = TransferBuffer.Handle,
			offset = Offset,
			pixels_per_row = ImagePitch,
			rows_per_layer = ImageHeight
		};
	}
}

#endregion