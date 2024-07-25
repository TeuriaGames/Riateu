namespace Riateu.Inputs;

public abstract class InputButton
{
    public InputState InputState;

    public bool Pressed => InputState.Pressed;
    public bool Held => InputState.Held;
    public bool IsDown => InputState.IsDown;
    public bool Released => InputState.Released;
    public bool Idle => InputState.Idle;
    public bool IsUp => InputState.IsUp;

    internal void Update() 
    {
        InputState.Update(Check());
    }

    protected abstract bool Check();
}
