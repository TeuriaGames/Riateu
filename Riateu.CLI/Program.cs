using System;
using System.IO;
using Hjson;
using Riateu.CLI;

if (args.Length == 0) 
{
    Console.WriteLine("[USAGE]: \n");
    Console.WriteLine("build - Build the Riateu Project");
    Console.WriteLine("run - Run the Riateu Project");
    Console.WriteLine("publish - Publish the Riateu Project");
    return;
}

string command = args[0];

string projectPath = string.Empty;
if (args.Length > 1) 
{
    projectPath = args[1];
    if (!File.Exists(projectPath)) 
    {
        Console.WriteLine($"The project file {projectPath} does not exists.");
        return;
    }
}
else 
{
    var files = Directory.GetFiles("./");
    foreach (var file in files) 
    {
        if (Path.HasExtension(".tproj")) 
        {
            projectPath = file;
            break;
        }
    }
}

if (projectPath == string.Empty) 
{
    Console.WriteLine("No project file found in this directory.");
}


switch (command) 
{
case "build":
    Build();
    break;
case "run":
    Build();
    break;
case "publish":
    Build();
    break;
}

void Build() 
{
    var proj = HjsonValue.Load(projectPath);
    var name = proj["Name"].Qs();
    var targetFramework = proj["TargetFramework"].Qs();

    ProjectFile projectFile = new ProjectFile();
    projectFile.Name = name;
    projectFile.TargetFramework = targetFramework;
    var xmlDoc = projectFile.ToXml();
    xmlDoc.Save("something.xml");
}