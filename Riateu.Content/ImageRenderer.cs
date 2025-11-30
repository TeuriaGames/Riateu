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

    // Move this to importer
    private string[] items = ["PNG", "QOI"];
    private int current;

    public ImageRenderer(GraphicsDevice device, Batch batch, ImGuiRenderer renderer) 
    {
        camera = new Camera(1024/3, 640/2);
        this.device = device;
        this.batch = batch;
        renderTarget = new RenderTarget(device, 320, 320, TextureFormat.R8G8B8A8_UNORM);
        renderTargetPtr = renderer.BindTexture(renderTarget);
    }

    public void SetActiveTexture(Texture texture) 
    {
        this.texture = texture;
        camera.Zoom = Vector2.One;
    }

    public void Update() 
    {
        if (texture == null) 
        {
            return;
        }
        // ImGuiIOPtr ptr = ImGui.GetIO();
        // if (ptr.WantCaptureMouse) 
        // {
        //     return;
        // }
        // camera.Position = -new Vector2((320 * 0.5f) - (texture.Width * 0.5f), (320 * 0.5f) - (texture.Height * 0.5f));
        // camera.Zoom = new Vector2(camera.Zoom.X + (Input.Mouse.WheelY * 0.1f), camera.Zoom.Y + (Input.Mouse.WheelY * 0.1f));
        // if (camera.Zoom.X <= 0) 
        // {
        //     camera.Zoom = new Vector2(0.1f, 0.1f);
        // }
    }

    public void Draw() 
    {
        if (texture == null) 
        {
            return;
        }

        ImGui.Image(renderTargetPtr, new Vector2(160, 160));

        ImGui.SameLine();
        ImGui.BeginGroup();
        float width = texture.Width;
        float height = texture.Height;

        ImGui.SetNextItemWidth(100);
        ImGui.LabelText("Size", $"{width}x{height}");

        ImGui.SetNextItemWidth(100);
        float zoom = camera.Zoom.X;
        if (ImGui.SliderFloat("Zoom", ref zoom, 0.1f, 2)) 
        {
            camera.Zoom = new Vector2(zoom);
        }
        ImGui.EndGroup();

        //TODO move this to Importer class
        ImGui.SetNextItemWidth(100);
        ImGui.Combo("Format", ref current, items, items.Length);
        ImGui.Button("Import");
    }

    public void Render()
    {
        if (texture == null) 
        {
            return;
        }
        batch.Begin(texture, DrawSampler.PointClamp, camera);
        batch.Draw(Vector2.Zero, Color.White);
        batch.End();

        RenderPass pass = device.BeginTarget(renderTarget, Color.Black, true);
        batch.Render(pass);
        device.EndTarget(pass);
    }
}