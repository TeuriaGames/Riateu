using MoonWorks.Graphics;

namespace Riateu;

public abstract class GameLoop 
{
    /// <summary>
    /// The game application.
    /// </summary>
    public GameApp GameInstance { get; }

    public GameLoop(GameApp gameApp) 
    {
        GameInstance = gameApp;
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
    /// <param name="buffer">A command buffer</param>
    /// <param name="backbuffer">The swapchain texture of the main window</param>
    public abstract void Render(CommandBuffer buffer, Texture backbuffer);
}