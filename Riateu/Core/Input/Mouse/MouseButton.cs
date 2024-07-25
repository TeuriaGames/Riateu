namespace Riateu.Inputs;

public class MouseButton(Mouse mouse, MouseButtonCode buttonCode, uint mask) : InputButton
{
    public Mouse BaseInput = mouse;
    public MouseButtonCode ButtonCode = buttonCode;
    public uint Mask = mask;

    protected override bool Check()
    {
        return (BaseInput.ButtonMask & Mask) != 0;
    }
}
