using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Riateu.Graphics;
using SDL3;

namespace Riateu;

public class Window : IDisposable
{
    public string Title { get; private set; }
    public IntPtr Handle { get; internal set; }

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
    private bool IsDisposed;

    private static Dictionary<uint, Window> windows = new Dictionary<uint, Window>();
    private static uint totalWindows;
    private static Stack<uint> freedWindowID = new Stack<uint>();

    public Action<uint, uint> OnSizeChange = delegate {};

    public Window(WindowSettings settings, SDL.SDL_WindowFlags flags) 
    {
        if (settings.WindowMode == WindowMode.Fullscreen) 
        {
            flags |= SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN;
        }
        else if (settings.WindowMode == WindowMode.BorderlessFullscreen) 
        {
            flags |= SDL.SDL_WindowFlags.SDL_WINDOW_BORDERLESS;
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

        IntPtr mode = SDL.SDL_GetDesktopDisplayMode(0);

        unsafe {
            SDL.SDL_DisplayMode *modePtr = (SDL.SDL_DisplayMode*)mode;

            Handle = SDL.SDL_CreateWindow(
                settings.Title,
                settings.WindowMode == WindowMode.Windowed ? (int)settings.Width : modePtr->w,
                settings.WindowMode == WindowMode.Windowed ? (int)settings.Height : modePtr->h,
                (ulong)flags
            );
        }
        int width = 0, height = 0;
        SDL.SDL_GetWindowSize(Handle, ref width, ref height);

        Width = (uint)width;
        Height = (uint)height;
        this.id = id;
    }

    public static Window CreateWindow(WindowSettings settings, BackendFlags backendFlags) 
    {
        SDL.SDL_WindowFlags flags = SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN;

        if (backendFlags != BackendFlags.D3D11) 
        {
            flags |= backendFlags switch 
            {
                BackendFlags.Vulkan => SDL.SDL_WindowFlags.SDL_WINDOW_VULKAN,
                BackendFlags.Metal => SDL.SDL_WindowFlags.SDL_WINDOW_METAL,
                _ => throw new Exception("Not Supported")
            };
        }

        Window window;
        if (freedWindowID.TryPop(out uint freedID)) 
        {
            window = new Window(settings, flags, freedID);
            windows.Add(window.id, window);
        }
        else 
        {
            window = new Window(settings, flags, totalWindows);
            windows.Add(window.id, window);
            totalWindows += 1;            
        }
        return window;
    }

    public void SetWindowSize(uint width, uint height) 
    {
        SDL.SDL_SetWindowSize(Handle, (int)width, (int)height);
        Width = width;
        Height = height;

        if (WindowMode == WindowMode.Windowed) 
        {
            // SDL.SDL_SetWindowPosition(Handle, SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED);
        }
    }
    public void SetScreenMode(WindowMode windowMode)
    {
        SDL.SDL_WindowFlags windowFlag = 0;
        bool fullscreen = false;

        if (windowMode == WindowMode.Fullscreen)
        {
            windowFlag = SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN;
            fullscreen = true;
        }
        else if (windowMode == WindowMode.BorderlessFullscreen)
        {
            windowFlag |= SDL.SDL_WindowFlags.SDL_WINDOW_BORDERLESS;
            windowFlag |= SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN;
            fullscreen = true;
        }

        // FIXME the flags
        SDL.SDL_SetWindowFullscreen(Handle, fullscreen);

        if (windowMode == WindowMode.Windowed)
        {
            // SDL.SDL_SetWindowPosition(Handle, SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED);
        }

        WindowMode = windowMode;
    }

    public void Show() 
    {
        SDL.SDL_ShowWindow(Handle);
    }

    internal void HandleSizeChanged(uint width, uint height) 
    {
        Width = width;
        Height = height;

        OnSizeChange?.Invoke(width, height);
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
    Fullscreen
}