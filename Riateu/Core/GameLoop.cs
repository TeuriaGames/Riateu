using Riateu.Graphics;

namespace Riateu;

/// <summary>
/// A game loop class which only contains the game loop and does not have many functionality unlike Scene.
/// </summary>
public abstract class GameLoop 
{
    /// <summary>
    /// The game application.
    /// </summary>
    public GameApp GameInstance { get; }

    public GraphicsDevice GraphicsDevice { get; private set; }

    /// <summary>
    /// Create the game loop instance.
    /// </summary>
    /// <param name="gameApp">The game application</param>
    public GameLoop(GameApp gameApp) 
    {
        GameInstance = gameApp;
        GraphicsDevice = gameApp.GraphicsDevice;
    }

    /// <summary>
    /// Begin your scene initialization.
    /// </summary>
    public abstract void Begin();

    /// <summary>
    /// End of the scene. Do your cleanup code here.
    /// </summary>
    public abstract void End();

    /// <summary>
    /// A method that runs on every update frame.
    /// </summary>
    /// <param name="delta">A delta time</param>
    public abstract void Update(double delta);

    /// <summary>
    /// A method that called during the draw loop. Do your draw calls here.
    /// </summary>
    /// <param name="backbuffer">A backbuffer target that contains API for rendering to backbuffer</param>
    public abstract void Render(RenderTarget backbuffer);
}