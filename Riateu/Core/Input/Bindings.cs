namespace Riateu;

/// <summary>
/// An interface for binding a mappings on keyboard or gamepad.
/// </summary>
public interface IBinding 
{
    /// <summary>
    /// Check if a button binding has pressed.
    /// </summary>
    /// <returns>True if the button binding has pressed</returns>
    bool Pressed();
    /// <summary>
    /// Check if a button binding has pressed once.
    /// </summary>
    /// <returns>True if the button binding has pressed once</returns>
    bool JustPressed();
    /// <summary>
    /// Check if a button binding has released.
    /// </summary>
    /// <returns>True if the button binding has released</returns>
    bool Released();
}

/// <summary>
/// An interface for binding a mappings on keyboard or gamepad and get its axis value when pressed.
/// </summary>
public interface IAxisBinding 
{
    /// <summary>
    /// Update a value of an axis.
    /// </summary>
    void Update();
    /// <summary>
    /// Get the axis value.
    /// </summary>
    /// <returns>
    /// -1 if the negative binding is pressed, 1 if the positive binding is pressed, or 0 if
    /// none of the binding is pressed.
    /// </returns>
    int GetValue();
}