using SDL3;

namespace Riateu.Inputs;

public class MouseButton(Mouse mouse, MouseButtonCode buttonCode, SDL.SDL_MouseButtonFlags mask) : InputButton
{
    public Mouse BaseInput = mouse;
    public MouseButtonCode ButtonCode = buttonCode;
    public SDL.SDL_MouseButtonFlags Mask = mask;

    internal void Update() 
    {
        InputState.Update((BaseInput.ButtonMask & Mask) != 0);
    }
}
