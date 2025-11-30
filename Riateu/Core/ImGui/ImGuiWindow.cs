using System;
using System.Runtime.InteropServices;
using ImGuiNET;
using Riateu.Graphics;
using SDL3;

namespace Riateu.ImGuiRend;

public class ImGuiWindow : IDisposable 
{
    private readonly GCHandle handle;
    private readonly GraphicsDevice device;
    private readonly Window window;

    private readonly ImGuiViewportPtr viewportPtr;

    public GraphicsDevice Device => device;
    public Window Window => window;

    public ImGuiWindow(GraphicsDevice device, Window parent, ImGuiViewportPtr viewportPtr) 
    {
        handle = GCHandle.Alloc(this);
        this.viewportPtr = viewportPtr;
        this.device = device;
        SDL.SDL_WindowFlags flags = SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN;
        if ((viewportPtr.Flags & ImGuiViewportFlags.NoTaskBarIcon) != 0)
        {
            flags |= SDL.SDL_WindowFlags.SDL_WINDOW_UTILITY;
        }
        if ((viewportPtr.Flags & ImGuiViewportFlags.NoDecoration) != 0)
        {
            flags |= SDL.SDL_WindowFlags.SDL_WINDOW_BORDERLESS;
        }
        else
        {
            flags |= SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE;
        }

        if ((viewportPtr.Flags & ImGuiViewportFlags.TopMost) != 0)
        {
            flags |= SDL.SDL_WindowFlags.SDL_WINDOW_ALWAYS_ON_TOP;
        }

        window = Window.CreateWindow(new WindowSettings(
            "No Title Yet", (uint)viewportPtr.Size.X, (uint)viewportPtr.Size.Y, WindowMode.Windowed), flags); 

        window.Resized += (_, _) => viewportPtr.PlatformRequestResize = true;
        window.Moved += () => viewportPtr.PlatformRequestMove = true;
        window.Closed += () => viewportPtr.PlatformRequestClose = true;

        viewportPtr.PlatformUserData = (IntPtr)handle;
        device.ClaimWindow(window, SwapchainComposition.SDR, PresentMode.Mailbox);
        SDL.SDL_SetWindowParent(window.Handle, parent.Handle);
    }

    public ImGuiWindow(GraphicsDevice device, ImGuiViewportPtr viewportPtr, Window window) 
    {
        handle = GCHandle.Alloc(this);
        this.device = device;
        this.viewportPtr = viewportPtr;
        this.window = window;
        viewportPtr.PlatformUserData = (IntPtr)handle;
    }

    public void Dispose() 
    {
        device.UnclaimWindow(window);
        window.Dispose();
        handle.Free();
    }
}