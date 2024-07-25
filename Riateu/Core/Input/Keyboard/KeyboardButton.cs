namespace Riateu.Inputs;

public class KeyboardButton(Keyboard keyboard, KeyCode code) : InputButton
{
    public Keyboard BaseInput = keyboard;
    public KeyCode KeyCode = code;

    protected override unsafe bool Check()
    {
        return (((byte*)BaseInput.State)[(int)KeyCode]) != 0;
    }
}
