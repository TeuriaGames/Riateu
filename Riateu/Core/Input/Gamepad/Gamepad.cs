using System;
using System.Numerics;
using SDL3;

namespace Riateu.Inputs;

public sealed class Gamepad
{
    private const int ButtonCount = (int)GamepadButtonCode.Touchpad + 1;
    private const int AxisCount = (int)GamepadAxisCode.RightTrigger + 1;
    private const int AxisButtonCount = (int)GamepadAxisButtonCode.RightTrigger + 1;

    internal uint InstanceID;
    internal IntPtr Handle;

    public bool Connected => Handle != IntPtr.Zero;
    public bool NotConnected => Handle == IntPtr.Zero;
    public string Name { get; private set; } = "";
    public int Slot { get; }
    public GamepadType Type { get; private set; }

    public float Deadzone { get; set; } = 0.15f;
    public float AxisButtonThreshold { get; set; } = 0.5f;

    public bool AnyPressed { get; private set; }
    public GamepadButton AnyPressedButton { get; private set; }

    public GamepadButton[] Buttons { get; }
    public GamepadAxisButton[] AxisButtons { get; }

    public Vector2 LeftStick => new Vector2(axes[0], axes[1]);
    public Vector2 RightStick => new Vector2(axes[2], axes[3]);
    public float LeftTrigger => axes[4];
    public float RightTrigger => axes[5];

    private readonly float[] axes = new float[AxisCount];

    internal Gamepad(int slot)
    {
        Slot = slot;
        Buttons = new GamepadButton[ButtonCount];
        for (int i = 0; i < ButtonCount; i++)
        {
            Buttons[i] = new GamepadButton(this, (GamepadButtonCode)i);
        }
        AxisButtons = new GamepadAxisButton[AxisButtonCount];
        for (int i = 0; i < AxisButtonCount; i++)
        {
            AxisButtons[i] = new GamepadAxisButton(this, (GamepadAxisButtonCode)i);
        }
    }

    internal void Open(IntPtr handle, uint instanceID)
    {
        Handle = handle;
        InstanceID = instanceID;
        Name = SDL.SDL_GetGamepadName(handle) ?? "";
        Type = (GamepadType)Math.Clamp((int)SDL.SDL_GetGamepadType(handle), 0, (int)GamepadType.JoyConPair);
        SDL.SDL_SetGamepadPlayerIndex(handle, Slot);
    }

    internal void Close()
    {
        Handle = IntPtr.Zero;
        InstanceID = 0;
        Name = "";
        Type = GamepadType.Unknown;
        Array.Clear(axes);
    }

    internal void Update()
    {
        AnyPressed = false;
        bool connected = Connected;

        for (int i = 0; i < ButtonCount; i++)
        {
            var button = Buttons[i];
            button.Update(connected && SDL.SDL_GetGamepadButton(Handle, (SDL.SDL_GamepadButton)i));
            if (button.Pressed)
            {
                AnyPressed = true;
                AnyPressedButton = button;
            }
        }

        for (int i = 0; i < AxisCount; i++)
        {
            axes[i] = connected ? ApplyDeadzone(Normalize(SDL.SDL_GetGamepadAxis(Handle, (SDL.SDL_GamepadAxis)i))) : 0f;
        }

        float t = AxisButtonThreshold;
        AxisButtons[0].Update(axes[0] <= -t);
        AxisButtons[1].Update(axes[0] >= t);
        AxisButtons[2].Update(axes[1] <= -t);
        AxisButtons[3].Update(axes[1] >= t);
        AxisButtons[4].Update(axes[2] <= -t);
        AxisButtons[5].Update(axes[2] >= t);
        AxisButtons[6].Update(axes[3] <= -t);
        AxisButtons[7].Update(axes[3] >= t);
        AxisButtons[8].Update(axes[4] >= t);
        AxisButtons[9].Update(axes[5] >= t);
    }

    private static float Normalize(short value) => Math.Max(value / 32767f, -1f);

    private float ApplyDeadzone(float value)
    {
        float abs = MathF.Abs(value);
        if (abs < Deadzone)
        {
            return 0f;
        }
        return MathF.Sign(value) * (abs - Deadzone) / (1f - Deadzone);
    }

    public GamepadButton Button(GamepadButtonCode code) => Buttons[(int)code];
    public GamepadAxisButton Button(GamepadAxisButtonCode code) => AxisButtons[(int)code];

    public bool IsPressed(GamepadButtonCode code) => Buttons[(int)code].Pressed;
    public bool IsHeld(GamepadButtonCode code) => Buttons[(int)code].Held;
    public bool IsDown(GamepadButtonCode code) => Buttons[(int)code].IsDown;
    public bool IsReleased(GamepadButtonCode code) => Buttons[(int)code].Released;
    public bool IsIdle(GamepadButtonCode code) => Buttons[(int)code].Idle;
    public bool IsUp(GamepadButtonCode code) => Buttons[(int)code].IsUp;

    public bool IsPressed(GamepadAxisButtonCode code) => AxisButtons[(int)code].Pressed;
    public bool IsHeld(GamepadAxisButtonCode code) => AxisButtons[(int)code].Held;
    public bool IsDown(GamepadAxisButtonCode code) => AxisButtons[(int)code].IsDown;
    public bool IsReleased(GamepadAxisButtonCode code) => AxisButtons[(int)code].Released;
    public bool IsIdle(GamepadAxisButtonCode code) => AxisButtons[(int)code].Idle;
    public bool IsUp(GamepadAxisButtonCode code) => AxisButtons[(int)code].IsUp;

    public float Axis(GamepadAxisCode code) => axes[(int)code];

    public bool HasButton(GamepadButtonCode code) =>
        Connected && SDL.SDL_GamepadHasButton(Handle, (SDL.SDL_GamepadButton)code);

    public bool HasAxis(GamepadAxisCode code) =>
        Connected && SDL.SDL_GamepadHasAxis(Handle, (SDL.SDL_GamepadAxis)code);

    public bool Rumble(float leftMotor, float rightMotor, uint durationMs) =>
        Connected && SDL.SDL_RumbleGamepad(Handle, ToMotor(leftMotor), ToMotor(rightMotor), durationMs);

    public bool RumbleTriggers(float left, float right, uint durationMs) =>
        Connected && SDL.SDL_RumbleGamepadTriggers(Handle, ToMotor(left), ToMotor(right), durationMs);

    public bool StopRumble() => Rumble(0, 0, 0);

    public bool SetLED(byte r, byte g, byte b) =>
        Connected && SDL.SDL_SetGamepadLED(Handle, r, g, b);

    private static ushort ToMotor(float value) => (ushort)(Math.Clamp(value, 0f, 1f) * 0xFFFF);
}
