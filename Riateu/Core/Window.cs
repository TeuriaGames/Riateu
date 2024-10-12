using System;
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

    public Window(WindowSettings settings, SDL.SDL_WindowFlags flags, uint id) 
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

        var modeID = SDL.SDL_GetPrimaryDisplay();

        unsafe {
            SDL.SDL_DisplayMode *modePtr = (SDL.SDL_DisplayMode*)SDL.SDL_GetCurrentDisplayMode(modeID);

            Handle = SDL.SDL_CreateWindow(
                settings.Title,
                settings.WindowMode == WindowMode.Windowed ? (int)settings.Width : modePtr->w,
                settings.WindowMode == WindowMode.Windowed ? (int)settings.Height : modePtr->h,
                flags
            );
        }
        SDL.SDL_GetWindowSize(Handle, out int width, out int height);

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
            var displayID = SDL.SDL_GetDisplayForWindow(Handle);
            unsafe {
                SDL.SDL_DisplayMode *modePtr = (SDL.SDL_DisplayMode*)SDL.SDL_GetCurrentDisplayMode(displayID);
                SDL.SDL_SetWindowPosition(Handle, modePtr->w / 2, modePtr->h / 2);
            }
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
            var displayID = SDL.SDL_GetDisplayForWindow(Handle);
            unsafe {
                SDL.SDL_DisplayMode *modePtr = (SDL.SDL_DisplayMode*)SDL.SDL_GetCurrentDisplayMode(displayID);
                SDL.SDL_SetWindowPosition(Handle, modePtr->w / 2, modePtr->h / 2);
            }
        }

        SDL.SDL_SyncWindow(Handle);

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
    Fullscreen,
}