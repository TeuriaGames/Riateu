namespace Riateu.Inputs;

public class GamepadButton(Gamepad gamepad, GamepadButtonCode code) : InputButton
{
    public Gamepad BaseInput = gamepad;
    public GamepadButtonCode ButtonCode = code;

    internal void Update(bool down) => InputState.Update(down);
}

public class GamepadAxisButton(Gamepad gamepad, GamepadAxisButtonCode code) : InputButton
{
    public Gamepad BaseInput = gamepad;
    public GamepadAxisButtonCode ButtonCode = code;

    internal void Update(bool down) => InputState.Update(down);
}
