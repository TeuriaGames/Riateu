using GpuBuffer = MoonWorks.Graphics.Buffer;
using System;
using System.Collections.Generic;
using ImGuiNET;
using MoonWorks;
using MoonWorks.Graphics;
using MoonWorks.Input;
using MoonWorks.Math.Float;
using Riateu.Graphics;
using Riateu.Misc;

namespace Riateu.ImGuiRend;

/// <summary>
/// A canvas renderer to render ImGui library.
/// </summary>
public class ImGuiRenderer
{
    private Dictionary<nint, Texture> PtrMap = new();
    private GraphicsPipeline imGuiPipeline;
    private Shader imGuiShader;
    private Sampler imGuiSampler;
    private GraphicsDevice device;
    private uint vertexCount;
    private uint indexCount;
    private GpuBuffer imGuiVertexBuffer;
    private GpuBuffer imGuiIndexBuffer;
    private TransferBuffer transferBuffer;

    /// <summary>
    /// A initilization for the ImGui renderer and to create its context.
    /// </summary>
    /// <param name="device">An application device</param>
    /// <param name="window">An application window</param>
    /// <param name="width">A width of the canvas</param>
    /// <param name="height">A height of the canvas</param>
    /// <param name="onInit">Called before building the font</param>
    public ImGuiRenderer(GraphicsDevice device, Window window, int width, int height, Action<ImGuiIOPtr> onInit = null)
    {
        this.device = device;
        ImGui.CreateContext();

        var io = ImGui.GetIO();
        io.DisplaySize = new System.Numerics.Vector2(width, height);
        io.DisplayFramebufferScale = System.Numerics.Vector2.One;
        imGuiShader = Resources.GetShader(device, Resources.ImGuiShader, "main", new ShaderCreateInfo {
            ShaderStage = ShaderStage.Vertex,
            ShaderFormat = ShaderFormat.SPIRV,
            UniformBufferCount = 1
        });
        imGuiSampler = new Sampler(device, SamplerCreateInfo.PointClamp);

        var fragmentShader = Resources.GetShader(device, Resources.Texture, "main", new ShaderCreateInfo {
            ShaderStage = ShaderStage.Fragment,
            ShaderFormat = ShaderFormat.SPIRV,
            SamplerCount = 1
        });

        imGuiPipeline = new GraphicsPipeline(
            device,
            new GraphicsPipelineCreateInfo
            {
                AttachmentInfo = new GraphicsPipelineAttachmentInfo(
                    new ColorAttachmentDescription(
                        window.SwapchainFormat,
                        ColorAttachmentBlendState.NonPremultiplied
                    )
                ),
                DepthStencilState = DepthStencilState.Disable,
                PrimitiveType = PrimitiveType.TriangleList,
                RasterizerState = RasterizerState.CCW_CullNone,
                MultisampleState = MultisampleState.None,
                VertexShader = imGuiShader,
                FragmentShader = fragmentShader,
                VertexInputState = VertexInputState.CreateSingleBinding<Position2DTextureColorVertex>()
            }
        );

        window.RegisterSizeChangeCallback(HandleSizeChanged);

        MoonWorks.Input.Inputs.TextInput += c =>
        {
            if (c == '\t') { return; }
            io.AddInputCharacter(c);
        };

        io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;

        if (!OperatingSystem.IsWindows())
        {
            io.SetClipboardTextFn = Clipboard.SetFnPtr;
            io.GetClipboardTextFn = Clipboard.GetFnPtr;
        }

        onInit?.Invoke(io);

        BuildFontAtlas();
    }

    private void HandleSizeChanged(uint width, uint height)
    {
        var io = ImGui.GetIO();
        io.DisplaySize = new System.Numerics.Vector2(width, height);
    }

    /// <summary>
    /// A method that updates ImGui inputs and for drawing.
    /// </summary>
    /// <param name="inputs">An application input device</param>
    /// <param name="imGuiCallback">A callback used for rendering</param>
    public void Update(MoonWorks.Input.Inputs inputs, Action imGuiCallback)
    {
        var io = ImGui.GetIO();

        io.MousePos = new System.Numerics.Vector2(inputs.Mouse.X, inputs.Mouse.Y);
        io.MouseDown[0] = inputs.Mouse.LeftButton.IsDown;
        io.MouseDown[1] = inputs.Mouse.RightButton.IsDown;
        io.MouseDown[2] = inputs.Mouse.MiddleButton.IsDown;
        io.MouseWheel = inputs.Mouse.Wheel;

        io.AddKeyEvent(ImGuiKey.A, inputs.Keyboard.IsDown(KeyCode.A));
        io.AddKeyEvent(ImGuiKey.Z, inputs.Keyboard.IsDown(KeyCode.Z));
        io.AddKeyEvent(ImGuiKey.Y, inputs.Keyboard.IsDown(KeyCode.Y));
        io.AddKeyEvent(ImGuiKey.X, inputs.Keyboard.IsDown(KeyCode.X));
        io.AddKeyEvent(ImGuiKey.C, inputs.Keyboard.IsDown(KeyCode.C));
        io.AddKeyEvent(ImGuiKey.V, inputs.Keyboard.IsDown(KeyCode.V));

        io.AddKeyEvent(ImGuiKey.Tab, inputs.Keyboard.IsDown(KeyCode.Tab));
        io.AddKeyEvent(ImGuiKey.LeftArrow, inputs.Keyboard.IsDown(KeyCode.Left));
        io.AddKeyEvent(ImGuiKey.RightArrow, inputs.Keyboard.IsDown(KeyCode.Right));
        io.AddKeyEvent(ImGuiKey.UpArrow, inputs.Keyboard.IsDown(KeyCode.Up));
        io.AddKeyEvent(ImGuiKey.DownArrow, inputs.Keyboard.IsDown(KeyCode.Down));
        io.AddKeyEvent(ImGuiKey.Enter, inputs.Keyboard.IsDown(KeyCode.Return));
        io.AddKeyEvent(ImGuiKey.Escape, inputs.Keyboard.IsDown(KeyCode.Escape));
        io.AddKeyEvent(ImGuiKey.Delete, inputs.Keyboard.IsDown(KeyCode.Delete));
        io.AddKeyEvent(ImGuiKey.Backspace, inputs.Keyboard.IsDown(KeyCode.Backspace));
        io.AddKeyEvent(ImGuiKey.Home, inputs.Keyboard.IsDown(KeyCode.Home));
        io.AddKeyEvent(ImGuiKey.End, inputs.Keyboard.IsDown(KeyCode.End));
        io.AddKeyEvent(ImGuiKey.PageDown, inputs.Keyboard.IsDown(KeyCode.PageDown));
        io.AddKeyEvent(ImGuiKey.PageUp, inputs.Keyboard.IsDown(KeyCode.PageUp));
        io.AddKeyEvent(ImGuiKey.Insert, inputs.Keyboard.IsDown(KeyCode.Insert));

        io.AddKeyEvent(ImGuiKey.ModCtrl, inputs.Keyboard.IsDown(KeyCode.LeftControl) || inputs.Keyboard.IsDown(KeyCode.RightControl));
        io.AddKeyEvent(ImGuiKey.ModShift, inputs.Keyboard.IsDown(KeyCode.LeftShift) || inputs.Keyboard.IsDown(KeyCode.RightShift));
        io.AddKeyEvent(ImGuiKey.ModAlt, inputs.Keyboard.IsDown(KeyCode.LeftAlt) || inputs.Keyboard.IsDown(KeyCode.RightAlt));
        io.AddKeyEvent(ImGuiKey.ModSuper, inputs.Keyboard.IsDown(KeyCode.LeftMeta) || inputs.Keyboard.IsDown(KeyCode.RightMeta));


        ImGui.NewFrame();
        imGuiCallback();
        ImGui.EndFrame();
    }

    /// <summary>
    /// A draw method used for rendering all of the drawn ImGui surface.
    /// </summary>
    /// <param name="renderPass">
    /// A renderpass used for handling and submitting a buffers
    /// </param>
    public void Draw(RenderPass renderPass)
    {;
        ImGui.Render();

        var io = ImGui.GetIO();
        var drawDataPtr = ImGui.GetDrawData();

        UpdateImGuiBuffers(drawDataPtr);

        RenderCommandLists(renderPass, drawDataPtr, io);
    }

    private unsafe void UpdateImGuiBuffers(ImDrawDataPtr drawDataPtr)
    {
        if (drawDataPtr.TotalVtxCount == 0) { return; }

        bool needNewTransferBuffer = false;

        var commandBuffer = device.AcquireCommandBuffer();

        if (drawDataPtr.TotalVtxCount > vertexCount)
        {
            imGuiVertexBuffer?.Dispose();

            vertexCount = (uint)(drawDataPtr.TotalVtxCount * 1.5f);
            imGuiVertexBuffer = GpuBuffer.Create<Position2DTextureColorVertex>(
                device,
                BufferUsageFlags.Vertex,
                vertexCount
            );
            needNewTransferBuffer = true;
        }

        if (drawDataPtr.TotalIdxCount > indexCount)
        {
            imGuiIndexBuffer?.Dispose();

            indexCount = (uint)(drawDataPtr.TotalIdxCount * 1.5f);
            imGuiIndexBuffer = GpuBuffer.Create<ushort>(
                device,
                BufferUsageFlags.Index,
                indexCount
            );
            needNewTransferBuffer = true;
        }

        if (needNewTransferBuffer)
        {
            transferBuffer?.Dispose();
            transferBuffer = new TransferBuffer(device, TransferBufferUsage.Upload, imGuiVertexBuffer.Size + imGuiIndexBuffer.Size);
        }
        uint vertexOffset = 0;
        uint indexOffset = 0;
        uint offset = 0;

        CopyPass copyPass = commandBuffer.BeginCopyPass();
        for (var n = 0; n < drawDataPtr.CmdListsCount; n += 1)
        {
            var cmdList = drawDataPtr.CmdLists[n];

            Span<Position2DTextureColorVertex> vertexSpan = new Span<Position2DTextureColorVertex>((void*)cmdList.VtxBuffer.Data, cmdList.VtxBuffer.Size);
            Span<ushort> indexSpan = new Span<ushort>((void*)cmdList.IdxBuffer.Data, cmdList.IdxBuffer.Size);

            uint length = transferBuffer.SetData(vertexSpan, offset, false);
            copyPass.UploadToBuffer(new TransferBufferLocation(transferBuffer, offset), new BufferRegion(imGuiVertexBuffer, vertexOffset, length), false);

            vertexOffset += (uint)length;
            offset += length;

            length = transferBuffer.SetData(indexSpan, offset, false);
            copyPass.UploadToBuffer(new TransferBufferLocation(transferBuffer, offset), new BufferRegion(imGuiIndexBuffer, indexOffset, length), false);

            offset += length;
            indexOffset += (uint)length;
        }

        commandBuffer.EndCopyPass(copyPass);

        device.Submit(commandBuffer);
    }

    private void RenderCommandLists(RenderPass renderPass, ImDrawDataPtr drawDataPtr, ImGuiIOPtr ioPtr)
    {
        var view = Matrix4x4.CreateLookAt(
            new Vector3(0, 0, 1),
            Vector3.Zero,
            Vector3.Up
        );

        var projection = Matrix4x4.CreateOrthographicOffCenter(
            0,
            480,
            270,
            0,
            0.01f,
            4000f
        );

        var viewProjectionMatrix = view * projection;

        renderPass.BindGraphicsPipeline(imGuiPipeline);

        renderPass.PushVertexUniformData(
            Matrix4x4.CreateOrthographicOffCenter(0, ioPtr.DisplaySize.X, 0, ioPtr.DisplaySize.Y, -1, 1)
        );

        renderPass.BindVertexBuffer(imGuiVertexBuffer);
        renderPass.BindIndexBuffer(imGuiIndexBuffer, IndexElementSize.Sixteen);

        uint vertexOffset = 0;
        uint indexOffset = 0;

        for (int n = 0; n < drawDataPtr.CmdListsCount; n += 1)
        {
            var cmdList = drawDataPtr.CmdLists[n];

            for (int cmdIndex = 0; cmdIndex < cmdList.CmdBuffer.Size; cmdIndex += 1)
            {
                var drawCmd = cmdList.CmdBuffer[cmdIndex];

                renderPass.BindFragmentSampler(
                    new TextureSamplerBinding(GetPointer(drawCmd.TextureId), imGuiSampler)
                );

                var topLeft = Vector2.Transform(new Vector2(drawCmd.ClipRect.X, drawCmd.ClipRect.Y), viewProjectionMatrix);
                var bottomRight = Vector2.Transform(new Vector2(drawCmd.ClipRect.Z, drawCmd.ClipRect.W), viewProjectionMatrix);

                var width = drawCmd.ClipRect.Z - (int)drawCmd.ClipRect.X;
                var height = drawCmd.ClipRect.W - (int)drawCmd.ClipRect.Y;

                if (width <= 0 || height <= 0)
                {
                    continue;
                }

                int x = drawCmd.ClipRect.X < 0 ? 0 : (int)drawCmd.ClipRect.X;
                int y = drawCmd.ClipRect.Y < 0 ? 0 : (int)drawCmd.ClipRect.Y;

                renderPass.SetScissor(
                    new Rect(
                        x, y,
                        (int)width,
                        (int)height
                    )
                );

                renderPass.DrawIndexedPrimitives(
                    vertexOffset,
                    indexOffset,
                    drawCmd.ElemCount / 3
                );

                indexOffset += drawCmd.ElemCount;
            }

            vertexOffset += (uint)cmdList.VtxBuffer.Size;
        }
    }

    /// <summary>
    /// Destroy the ImGui context.
    /// </summary>
    public void Destroy()
    {
        ImGui.DestroyContext();
    }

    private unsafe void BuildFontAtlas()
    {
        var cmdBuf = device.AcquireCommandBuffer();

        var io = ImGui.GetIO();

        io.Fonts.GetTexDataAsRGBA32(
            out nint pixelData,
            out int width,
            out int height,
            out int bytesPerPixel
        );


        using var uploader = new ResourceUploader(device);
        var fontTexture = uploader.CreateTexture2D(new Span<byte>((void*)pixelData, width * height * bytesPerPixel), (uint)width, (uint)height);

        uploader.Upload();

        io.Fonts.SetTexID(fontTexture.Handle);
        io.Fonts.ClearTexData();

        BindTexture(fontTexture);
    }

    public IntPtr BindTexture(Texture texture)
    {
        if (!PtrMap.ContainsKey(texture.Handle))
        {
            PtrMap.Add(texture.Handle, texture);
        }

        return texture.Handle;
    }

    public void UnbindTexture(IntPtr ptr)
    {
        if (!PtrMap.ContainsKey(ptr))
        {
            PtrMap.Remove(ptr);
        }
    }

    public Texture GetPointer(IntPtr ptr)
    {
        if (!PtrMap.ContainsKey(ptr))
        {
            return null;
        }

        var texture = PtrMap[ptr];

        return texture;
    }
}
