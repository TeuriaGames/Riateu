using System;
using System.IO;
using ImGuiNET;
using NativeFileDialogSharp;

public class AssetsContainer 
{
    public Action<string> OnAssetSelected;
    private string pathSelected;

    public AssetsContainer(Action<string> assetSelected) 
    {
        OnAssetSelected = assetSelected;
    }

    public void Draw() 
    {
        ImGui.Begin("Assets");
        if (pathSelected == null) 
        {
            ImGui.LabelText("##Notice", "No Project selected");
            if (ImGui.Button("Select a Project")) 
            {
                DialogResult dialog = Dialog.FolderPicker("./");
                if (dialog.IsOk) 
                {
                    pathSelected = dialog.Path;
                }
            }
        }
        else 
        {
            FileSystem(pathSelected);
        }

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