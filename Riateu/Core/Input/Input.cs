namespace Riateu.Inputs;

public static class Input 
{
    public static bool Disabled => Device.Disabled;
    public static InputDevice Device { get; internal set; }

    public static Keyboard Keyboard => Device.Keyboard;
    public static Mouse Mouse => Device.Mouse;
    public static Gamepad[] Gamepads => Device.Gamepads;

    public static Gamepad Gamepad(int slot) => Device.Gamepad(slot);

    internal static void Init(InputDevice inputDevice) 
    {
        Device = inputDevice;
    }
}
