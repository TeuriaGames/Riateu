using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using MoonWorks;

namespace Riateu.Content;

/// <summary>
/// A class to build your content.
/// </summary>
public class ContentBuilder 
{
    private StringBuilder logBuilder = new StringBuilder();
    private Dictionary<Type, ContentProcessor> Processors = new Dictionary<Type, ContentProcessor>();
    private string destination;

    /// <summary>
    /// Initialize the <see cref="Riateu.Content.ContentBuilder"/>
    /// </summary>
    /// <param name="destination"></param>
    public ContentBuilder(string destination) 
    {
        this.destination = destination;
        if (!Directory.Exists(destination)) 
        {
            Directory.CreateDirectory(destination);
        }
    }

    /// <summary>
    /// Add a processor to the content builder.
    /// </summary>
    /// <param name="processor">A <see cref="Riateu.Content.ContentProcessor"/></param>
    /// <typeparam name="T">A type of <see cref="Riateu.Content.ContentProcessor"/></typeparam>
    public void AddProcessor<T>(T processor) 
    where T : ContentProcessor
    {
        Processors.Add(typeof(T), processor);
    }

    /// <summary>
    /// Process all files from a folder with a given processor.
    /// </summary>
    /// <param name="folder">A folder to process with</param>
    public void ProcessBulk<T>(string folder) 
    {
        if (Processors.TryGetValue(typeof(T), out var contentProcessor)) 
        {
            var files = Directory.GetFiles(folder);
            foreach (var file in files) 
            {
                InternalProcess(file, contentProcessor);
            }
            return;
        }
        Logger.LogError($"No content processor: '{typeof(T).Name}' was found");
    }

    /// <summary>
    /// Process a file or folder with a given processor.
    /// </summary>
    /// <param name="path">A path to process with</param>
    public void Process<T>(string path) 
    where T : ContentProcessor
    {
        if (Processors.TryGetValue(typeof(T), out var contentProcessor)) 
        {
            InternalProcess(path, contentProcessor);
            return;
        }
        Logger.LogError($"No content processor: '{typeof(T).Name}' was found");
    }

    internal void InternalProcess(string file, ContentProcessor contentProcessor) 
    {
        try 
        {
            contentProcessor.Init(logBuilder);
            if (contentProcessor.DirectoriesToEnsure != null) 
            {
                for (int i = 0; i < contentProcessor.DirectoriesToEnsure.Length; i++) 
                {
                    var directory = Path.Combine(destination, contentProcessor.DirectoriesToEnsure[i]);
                    if (!Directory.Exists(directory)) 
                    {
                        Directory.CreateDirectory(directory);
                    }
                }
            }
            contentProcessor.Process(file, destination);
        }
        catch (Exception ex)
        {
            Logger.LogError("Error while processing a content");
            Logger.LogError(ex.ToString());
        }
    }

    /// <summary>
    /// Create a log file from the previous logs.
    /// </summary>
    /// <param name="filePath">A destination path to create</param>
    public void CreateLogs(string filePath = null) 
    {
        using var fs = File.Create(filePath ?? "contentbuilder-log.txt");
        using TextWriter tw = new StreamWriter(fs);

        var text = logBuilder.ToString();

        tw.Write(text);
    }

    ///
    ~ContentBuilder() 
    {
        CreateLogs();
    }
}

/// <summary>
/// A base class for the content processor.
/// </summary>
public abstract class ContentProcessor : IDisposable
{
    /// <summary>
    /// Ensure these directory does exists.
    /// </summary>
    public virtual string[] DirectoriesToEnsure { get; }
    private Process process;
    private StringBuilder logBuilder;

    internal void Init(StringBuilder logBuilder) 
    {
        process = new Process();
        this.logBuilder = logBuilder;
    }

    /// <summary>
    /// Dispose a process.
    /// </summary>
    public void Dispose()
    {
        process.Dispose();
    }

    /// <summary>
    /// Process a file into a readable content.
    /// </summary>
    /// <param name="filePath">A path from a target file</param>
    /// <param name="outputDir">A target destination of content</param>
    /// <returns>A processed content object</returns>
    public abstract void Process(string filePath, string outputDir);

    /// <summary>
    /// Run a process or a command.
    /// </summary>
    /// <param name="command">A command or process to run</param>
    /// <param name="args">Arguments to a command or process</param>
    public int RunCommand(string command, string[] args) 
    {
        process.StartInfo = new ProcessStartInfo(command, args);
        var success = process.Start();
        process.WaitForExit();
        return process.ExitCode;
    }

    /// <summary>
    /// Log a message to the console.
    /// </summary>
    /// <param name="message">A message to output</param>
    public void Log(string message) 
    {
        var text = $"[{this.GetType().Name}] {message}";
        Logger.LogInfo(text);
        logBuilder.AppendLine(text);
    }
}