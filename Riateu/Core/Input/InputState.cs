namespace Riateu.Inputs;

public struct InputState 
{
    public Status InputStatus { get; private set; }
    public bool Pressed => InputStatus is Status.Pressed;
    public bool IsDown => InputStatus is Status.Pressed or Status.Held;
    public bool Held => InputStatus is Status.Held;
    public bool Released => InputStatus is Status.Released;
    public bool IsUp => InputStatus is Status.Released or Status.Idle;
    public bool Idle => InputStatus is Status.Idle;

    internal void Update(bool IsPressed) 
    {
        if (IsPressed) 
        {
            if (IsUp) 
            {
                InputStatus = Status.Pressed;
            }
            else 
            {
                InputStatus = Status.Held;
            }
            return;
        }
        if (IsDown) 
        {
            InputStatus = Status.Released;
            return;
        }
        InputStatus = Status.Idle;
    }

    public enum Status 
    {
        Idle,
        Pressed,
        Held,
        Released
    }
}
