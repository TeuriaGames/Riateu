using Riateu;
using Riateu.Content.App;

public class Program : GameApp
{
    public Program(WindowSettings settings, GraphicsSettings graphicsSettings) : base(settings, graphicsSettings)
    {
    }

    public override GameLoop Initialize()
    {
        return new ContentWindow(this);
    }
    
    public static void Main(string[] args) 
    {
        Program program = new Program(
            new WindowSettings("Content Manager", 1024, 640, WindowMode.Windowed),
            GraphicsSettings.Vsync
        );
        program.Run();
    }
}
