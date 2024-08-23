using System;
using System.Numerics;
using ImGuiNET;
using Riateu.Graphics;
using Riateu.ImGuiRend;

namespace Riateu.Content.App;

public class ImageRenderer 
{
    private RenderTarget renderTarget;
    private Texture texture;
    private IntPtr renderTargetPtr;
    private GraphicsDevice device;
    private Batch batch;
    private Camera camera;
    public ImageRenderer(GraphicsDevice device, Batch batch, ImGuiRenderer renderer) 
    {
        camera = new Camera(1024/3, 640/2);
        this.device = device;
        this.batch = batch;
        renderTarget = new RenderTarget(device, 320, 320, TextureFormat.R8G8B8A8);
        renderTargetPtr = renderer.BindTexture(renderTarget);
    }

    public void SetActiveTexture(Texture texture) 
    {
        this.texture = texture;
    }

    public void Draw() 
    {
        if (texture != null) 
        {
            batch.Begin(texture, DrawSampler.PointClamp, camera);
            batch.Draw(Vector2.Zero, Color.White);
            batch.End();
        }

        ImGui.Image(renderTargetPtr, new Vector2(320, 320));
    }

    public void Render() 
    {
        RenderPass pass = device.BeginTarget(renderTarget, Color.Black, true);
        batch.Render(pass);
        device.EndTarget(pass);
    }
}