using System;
using Riateu.Graphics;
using SDL2;

namespace Riateu;

public class Window : IDisposable
{
    public string Title { get; private set; }
    public IntPtr Handle { get; internal set; }

    public uint Width { get; private set; }
    public uint Height { get; private set; }

    public WindowMode WindowMode => windowMode;
    private WindowMode windowMode;

    public SwapchainComposition SwapchainComposition { get; internal set; }
    public RenderTarget SwapchainTarget { get; internal set; }
    public TextureFormat SwapchainFormat { get; internal set; }

    public bool Claimed { get; internal set; }
    private bool IsDisposed;

    public Window(WindowSettings settings, SDL.SDL_WindowFlags flags) 
    {
        if (settings.WindowMode == WindowMode.Fullscreen) 
        {
            flags |= SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN;
        }
        else if (settings.WindowMode == WindowMode.BorderlessFullscreen) 
        {
            flags |= SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP;
        }

        if (settings.Flags.Resizable) 
        {
            flags |= SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE;
        }

        if (settings.Flags.StartMaximized) 
        {
            flags |= SDL.SDL_WindowFlags.SDL_WINDOW_MAXIMIZED;
        }

        this.windowMode = settings.WindowMode;

        SDL.SDL_GetDesktopDisplayMode(0, out var mode);

        Handle = SDL.SDL_CreateWindow(
            settings.Title,
            SDL.SDL_WINDOWPOS_CENTERED,
            SDL.SDL_WINDOWPOS_CENTERED,
            settings.WindowMode == WindowMode.Windowed ? (int)settings.Width : mode.w,
            settings.WindowMode == WindowMode.Windowed ? (int)settings.Height : mode.h,
            flags
        );

        SDL.SDL_GetWindowSize(Handle, out int width, out int height);

        Width = (uint)width;
        Height = (uint)height;
    }

    internal void Show() 
    {
        SDL.SDL_ShowWindow(Handle);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!IsDisposed)
        {
            SDL.SDL_DestroyWindow(Handle);
            IsDisposed = true;
        }
    }

    ~Window()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

public record struct WindowSettings(string Title, uint Width, uint Height, WindowMode WindowMode = WindowMode.Windowed, Flags Flags = default);

public record struct Flags(bool Resizable = false, bool StartMaximized = false);

public enum WindowMode 
{
    Windowed,
    Fullscreen,
    BorderlessFullscreen
}