using System;
using System.Collections.Generic;
using System.Threading.Channels;
using Riateu.Graphics;
using SDL3;

namespace Riateu;

public class Window : IDisposable
{
    public string Title 
    { 
        get => title;
        set => SDL.SDL_SetWindowTitle(Handle, value);
    }
    public IntPtr Handle { get; internal set; }
    private string title;

    public uint Width { get; private set; }
    public uint Height { get; private set; }

    public WindowMode WindowMode 
    {
        get => windowMode;
        set => windowMode = value;
    }
    private WindowMode windowMode;
    private uint id;

    public SwapchainComposition SwapchainComposition { get; internal set; }
    public RenderTarget SwapchainTarget { get; internal set; }
    public TextureFormat SwapchainFormat { get; internal set; }

    public bool Claimed { get; internal set; }
    public static int TotalWindowCount => windows.Count;

    public event Action Moved;
    public event Action Closed;
    private bool IsDisposed;

    private static Dictionary<uint, Window> windows = new Dictionary<uint, Window>();

    public static IReadOnlyDictionary<uint, Window> Windows => windows;

    public event Action<uint, uint> Resized = delegate {};

    private unsafe Window(WindowSettings settings, SDL.SDL_WindowFlags flags) 
    {
        if (settings.WindowMode == WindowMode.Fullscreen) 
        {
            flags |= SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN;
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

        title = settings.Title;

        if (settings.WindowMode == WindowMode.Windowed) 
        {
            Handle = SDL.SDL_CreateWindow(
                title,
                (int)settings.Width,
                (int)settings.Height,
                flags
            );

            Width = settings.Width;
            Height = settings.Height;
        }
        else 
        {
            var modeID = SDL.SDL_GetPrimaryDisplay();
            SDL.SDL_DisplayMode *modePtr = (SDL.SDL_DisplayMode*)SDL.SDL_GetCurrentDisplayMode(modeID);

            Handle = SDL.SDL_CreateWindow(
                title,
                modePtr->w,
                modePtr->h,
                flags
            );
            int width = 0;
            int height = 0;
            SDL.SDL_GetWindowSize(Handle, out width, out height);

            Width = (uint)width;
            Height = (uint)height;
        }

        this.id = SDL.SDL_GetWindowID(Handle);
    }

    public static Window CreateWindow(WindowSettings settings, SDL.SDL_WindowFlags flags, BackendFlags backendFlags) 
    {
        if (backendFlags != BackendFlags.DirectX) 
        {
            flags |= backendFlags switch 
            {
                BackendFlags.Vulkan => SDL.SDL_WindowFlags.SDL_WINDOW_VULKAN,
                BackendFlags.Metal => SDL.SDL_WindowFlags.SDL_WINDOW_METAL,
                _ => throw new Exception("Not Supported")
            };
        }

        Window window = new Window(settings, flags);
        windows.Add(window.id, window);
        return window;
    }

    public static Window CreateWindow(WindowSettings settings, BackendFlags backendFlags) 
    {
        SDL.SDL_WindowFlags flags = SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN;

        if (backendFlags != BackendFlags.DirectX) 
        {
            flags |= backendFlags switch 
            {
                BackendFlags.Vulkan => SDL.SDL_WindowFlags.SDL_WINDOW_VULKAN,
                BackendFlags.Metal => SDL.SDL_WindowFlags.SDL_WINDOW_METAL,
                _ => throw new Exception("Not Supported")
            };
        }

        Window window = new Window(settings, flags);
        windows.Add(window.id, window);
        return window;
    }

    public void SetWindowSizeRelative(uint width, uint height) 
    {
        SDL.SDL_SetWindowSize(Handle, (int)width, (int)height);
        Width = width;
        Height = height;
    }

    public void SetWindowSize(uint width, uint height) 
    {
        SDL.SDL_SetWindowSize(Handle, (int)width, (int)height);
        Width = width;
        Height = height;

        if (WindowMode == WindowMode.Windowed) 
        {
            SDL.SDL_SetWindowPosition(Handle, (int) 0x2FFF0000u, (int) 0x2FFF0000u);
        }
    }
    public void SetScreenMode(WindowMode windowMode)
    {
        if (windowMode == WindowMode.Fullscreen)
        {
            SDL.SDL_SetWindowFullscreen(Handle, true);
        }
        else
        {
            SDL.SDL_SetWindowPosition(Handle, (int) 0x2FFF0000u, (int) 0x2FFF0000u);
        }

        SDL.SDL_SyncWindow(Handle);

        WindowMode = windowMode;
    }

    public void Show() 
    {
        SDL.SDL_ShowWindow(Handle);
    }

    public void Move() 
    {
        Moved?.Invoke();
    }

    internal void HandleSizeChanged(uint width, uint height) 
    {
        Width = width;
        Height = height;

        Resized?.Invoke(width, height);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!IsDisposed)
        {
            windows.Remove(id);
            Closed?.Invoke();
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
        GC.SuppressFinalize(SwapchainTarget);
    }
}

public record struct WindowSettings(string Title, uint Width, uint Height, WindowMode WindowMode = WindowMode.Windowed, Flags Flags = default);

public record struct Flags(bool Resizable = false, bool StartMaximized = false);

public enum WindowMode 
{
    Windowed,
    Fullscreen,
}