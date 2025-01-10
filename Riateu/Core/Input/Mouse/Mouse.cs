using System.Drawing;
using System.Numerics;
using SDL3;

namespace Riateu.Inputs;

public class Mouse 
{
    public bool AnyPressed { get; private set; }
    public MouseButton AnyPressedButton { get; private set; }
    public MouseButton LeftButton { get; }
    public MouseButton MiddleButton { get; }
    public MouseButton RightButton { get; }
    public MouseButton X1Button { get; }
    public MouseButton X2Button { get; }

    public int X { get; private set; }
    public int Y { get; private set; }
    public int DeltaX { get; private set; }
    public int DeltaY { get; private set; }

    public Point Position => new Point(X, Y);
    public Vector2 PositionF => new Vector2(X, Y);

    public int WheelX { get; private set; }
    internal int WheelRawX;
    public int WheelY { get; private set; }
    internal int WheelRawY;
    private int previousWheelRawX = 0;
    private int previousWheelRawY = 0;
    private MouseButton[] MouseButtons = new MouseButton[5];

    internal SDL.SDL_MouseButtonFlags ButtonMask { get; private set; }

    private bool relativeMode;

    public bool RelativeMode
    {
        get => relativeMode;
        set
        {
            relativeMode = value;
            // FIXME RelativeMode
            // SDL.SDL_SetRelativeMouseMode(relativeMode ?  SDL.SDL_bool.SDL_TRUE : SDL.SDL_bool.SDL_FALSE);
        }
    }

    private bool hidden;

    public bool Hidden
    {
        get => hidden;
        set
        {
            hidden = value;
            if (hidden) 
            {
                SDL.SDL_HideCursor();
                return;
            } 
            SDL.SDL_ShowCursor();
        }
    }


    internal Mouse()
    {
        LeftButton = new MouseButton(this, MouseButtonCode.Left, SDL.SDL_MouseButtonFlags.SDL_BUTTON_LMASK);
        MiddleButton = new MouseButton(this, MouseButtonCode.Middle, SDL.SDL_MouseButtonFlags.SDL_BUTTON_MMASK);
        RightButton = new MouseButton(this, MouseButtonCode.Right, SDL.SDL_MouseButtonFlags.SDL_BUTTON_RMASK);
        X1Button = new MouseButton(this, MouseButtonCode.X1, SDL.SDL_MouseButtonFlags.SDL_BUTTON_X1MASK);
        X2Button = new MouseButton(this, MouseButtonCode.X2, SDL.SDL_MouseButtonFlags.SDL_BUTTON_X2MASK);

        MouseButtons[0] = LeftButton;
        MouseButtons[1] = RightButton;
        MouseButtons[2] = MiddleButton;
        MouseButtons[3] = X1Button;
        MouseButtons[4] = X2Button;
    }

    internal void Update()
    {
        AnyPressed = false;

        float x = 0;
        float y = 0;
        float deltaX = 0;
        float deltaY = 0;
        ButtonMask = SDL.SDL_GetMouseState(out x, out y);
        SDL.SDL_GetRelativeMouseState(out deltaX, out deltaY);

        X = (int)x;
        Y = (int)y;
        DeltaX = (int)deltaX;
        DeltaY = (int)deltaY;

        WheelX = WheelRawX - previousWheelRawX;
        previousWheelRawX = WheelRawX;

        WheelY = WheelRawY - previousWheelRawY;
        previousWheelRawY = WheelRawY;

        foreach (var button in MouseButtons)
        {
            button.Update();

            if (button.Pressed)
            {
                AnyPressed = true;
                AnyPressedButton = button;
            }
        }
    }

    public void ShowCursor(bool show) 
    {
        if (show) 
        {
            SDL.SDL_ShowCursor();
            return;
        }
        SDL.SDL_HideCursor();
    }
}
