using System;
using System.IO;
using ImGuiNET;

namespace Riateu.Content.App;

public class AssetsContainer 
{
    public Action<string> OnAssetSelected;
    public Action OnSelectProject;
    private ContentWindow window;

    public AssetsContainer(ContentWindow window) 
    {
        this.window = window;
    }

    public void Draw() 
    {
        ImGui.Begin($"{FA6.Box} Assets");
        if (string.IsNullOrEmpty(window.SelectedProject)) 
        {
            ImGui.LabelText("##Notice", "No Project selected");
            if (ImGui.Button($"{FA6.FileImport} Select a Project")) 
            {
                OnSelectProject?.Invoke();
            }
        }
        else 
        {
            FileSystem(window.SelectedProject);
        }

        ImGui.End();
    }

    private void FileSystem(string directory) 
    {
        string[] directories = Directory.GetDirectories(directory);
        string[] files = Directory.GetFiles(directory);

        foreach (var dir in directories) 
        {
            if (ImGui.TreeNode(FA6.Folder + " " + Path.GetFileName(dir))) 
            {
                FileSystem(dir);
                ImGui.TreePop();
            }
        }

        foreach (var file in files) 
        {
            if (ImGui.Selectable(GetIconName(file))) 
            {
                OnAssetSelected(file);
            }
        }
    }

    private string GetIconName(ReadOnlySpan<char> file) 
    {
        var spanFile = Path.GetFileName(file);
        if (file.EndsWith("png")) 
        {
            return $"{FA6.Image} {spanFile}";
        }
        if (file.EndsWith("ttf")) 
        {
            return $"{FA6.Font} {spanFile}";
        }
        return new string($"{FA6.File} {spanFile}");
    }
}