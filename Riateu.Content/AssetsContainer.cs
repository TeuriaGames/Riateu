using System;
using System.IO;
using ImGuiNET;

public class AssetsContainer 
{
    public Action<string> OnAssetSelected;

    public AssetsContainer(Action<string> assetSelected) 
    {
        OnAssetSelected = assetSelected;
    }

    public void Draw() 
    {
        ImGui.Begin("Assets");
        ImGui.LabelText("##Notice", "No Project selected");
        ImGui.Button("Select a Project");
        FileSystem("./");

        ImGui.End();
    }

    private void FileSystem(string directory) 
    {
        string[] directories = Directory.GetDirectories(directory);
        string[] files = Directory.GetFiles(directory);

        foreach (var dir in directories) 
        {
            if (ImGui.TreeNode(Path.GetFileName(dir))) 
            {
                FileSystem(dir);
                ImGui.TreePop();
            }
        }

        foreach (var file in files) 
        {
            if (ImGui.Selectable(Path.GetFileName(file))) 
            {
                OnAssetSelected(file);
            }
        }
    }
}