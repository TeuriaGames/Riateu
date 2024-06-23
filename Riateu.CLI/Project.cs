using System.Collections.Generic;
using System.Xml;

namespace Riateu.CLI;

public class ProjectFile 
{
    public string Name;
    public string TargetFramework;
    public string SdkPath;
    public Dictionary<string, string> NugetDependencies;
    public Dictionary<string, string> NativeDependencies;

    public XmlDocument ToXml() 
    {
        XmlDocument doc = new XmlDocument();
        XmlElement project = doc.CreateElement("Project");
        project.SetAttribute("Sdk", "Microsoft.NET.Sdk");
        doc.AppendChild(project);

        XmlElement propertyGroup = doc.CreateElement("PropertyGroup");
        project.AppendChild(propertyGroup);

        XmlElement assemblyName = doc.CreateElement("AssemblyName");
        assemblyName.InnerText = Name.Replace(" ", "");
        propertyGroup.AppendChild(assemblyName);

        XmlElement targetFramework = doc.CreateElement("TargetFramework");
        targetFramework.InnerText = TargetFramework;
        propertyGroup.AppendChild(targetFramework);

        return doc;
    }
}
