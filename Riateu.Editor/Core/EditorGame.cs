using MoonWorks;

namespace Riateu.Editor;


public class EditorGame : GameApp
{
    public EditorGame(string title, uint width, uint height, ScreenMode screenMode = ScreenMode.Windowed, bool debugMode = false) : base(title, width, height, screenMode, debugMode)
    {
    }

    public override void Initialize()
    {
        Scene = new EditorScene(this);
    }
}