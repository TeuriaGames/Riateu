using System;
using ImGuiNET;
using MoonWorks.Graphics;
using Riateu;
using Riateu.Graphics;
using Riateu.ImGuiRend;

namespace Riateu.Editor;

public class EditorScene : Scene
{
    private ImGuiRenderer renderer;

    public EditorScene(GameApp game) : base(game)
    {
        renderer = new ImGuiRenderer(GameInstance.GraphicsDevice, GameInstance.MainWindow, 1024, 640);
    }

    public override void Begin()
    {
    }

    public override void Update(double delta)
    {
        renderer.Update(GameInstance.Inputs, RenderImGui);
    }

    private void RenderImGui()
    {
        ImGui.ShowDemoWindow();
    }

    public override void Draw(CommandBuffer buffer, Texture backbuffer, IBatch batch)
    {
        buffer.BeginRenderPass(new ColorAttachmentInfo(backbuffer, Color.Black));

        // ImGui bind their own pipeline
        renderer.Draw(buffer);
        buffer.EndRenderPass();
    } 


    public override void End()
    {
    }
}