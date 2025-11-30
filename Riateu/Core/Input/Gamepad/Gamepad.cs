using System;
using SDL3;

namespace Riateu;

public sealed class Gamepad 
{
    internal uint InstanceID;
    internal IntPtr Handle;

    public bool NotConnected => Handle == IntPtr.Zero;
    public string Name { get; private set; }
    public int Slot { get; private set; }


    internal Gamepad() 
    {

    }

    internal void Open(IntPtr handle) 
    {
        Handle = handle;

        IntPtr joystickHandle = SDL.SDL_GetGamepadJoystick(Handle);
        InstanceID = SDL.SDL_GetJoystickID(joystickHandle);

        Name = SDL.SDL_GetGamepadName(Handle);
    }

    internal void Close() 
    {
        Handle = IntPtr.Zero;
        InstanceID = 0;
    }

    internal void Update() 
    {
        if (NotConnected)
        {
            return;
        }
    }

    public bool Rumble(float leftMotor, float rightMotor, uint duration) 
    {
        return SDL.SDL_RumbleGamepad(
            Handle, 
            (ushort)(Math.Clamp(leftMotor, 0.0f, 1.0f) * 0xFFFF), 
            (ushort)(Math.Clamp(rightMotor, 0.0f, 1.0f) * 0xFFFF), 
            duration / 1000
        );
    }
}
