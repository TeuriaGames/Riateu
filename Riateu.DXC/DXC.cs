using System.IO;
using System.Runtime.CompilerServices;
using DirectXShaderCompiler.NET;
using Riateu.Content;
using Riateu.Graphics;

namespace Riateu.DXC;

public static class DXC
{
    public static Shader CompileVertex(GraphicsDevice device, string filename, ShaderCreateInfo info)
    {
        var code = ReadCodeFromFile(filename);        
        var bytecode = CompileBytecode(code, GraphicsDevice.Backend, ShaderProfile.Vertex_6_0, out var format);
        return new Shader(device, bytecode, "main", info with 
        {
            ShaderStage = ShaderStage.Vertex,
            ShaderFormat = format
        });
    }

    public static Shader CompileFragment(GraphicsDevice device, string filename, ShaderCreateInfo info)
    {
        var code = ReadCodeFromFile(filename);        
        var bytecode = CompileBytecode(code, GraphicsDevice.Backend, ShaderProfile.Fragment_6_0, out var format);
        return new Shader(device, bytecode, "main", info with 
        {
            ShaderStage = ShaderStage.Fragment,
            ShaderFormat = format
        });
    }

    public static ComputePipeline CompileCompute(GraphicsDevice device, string filename, ComputePipelineCreateInfo info)
    {
        var code = ReadCodeFromFile(filename);        
        var bytecode = CompileBytecode(code, GraphicsDevice.Backend, ShaderProfile.Compute_6_0, out var format);
        return new ComputePipeline(device, bytecode, "main", info with 
        {
            ShaderFormat = format
        });
    }

    private static string ReadCodeFromFile(string filename)
    {
        string code;
        using (var fs = File.OpenRead(filename))
        {
            using (TextReader tr = new StreamReader(fs))
            {
                code = tr.ReadToEnd();
            }    
        }

        return code;
    }

    private static byte[] CompileBytecode(string code, string backend, ShaderProfile profile, out ShaderFormat format)
    {
        bool generateAsSpirv = GenerateAsSpirv(backend);
        CompilerOptions options = new CompilerOptions(profile)
        {
            entryPoint = "main",
            generateAsSpirV = generateAsSpirv,
        };

        CompilationResult result = ShaderCompiler.Compile(code, options);

        foreach (var message in result.messages)
        {
            Logger.Info($"{message.severity}: {message.message}");

            foreach (var file in message.stackTrace)
            {
                Logger.Error($"\tAt {file.filename}:{file.line}:{file.column}");
            }
        }

        if (generateAsSpirv)
        {
            format = ShaderFormat.SPIRV;
        }
        else 
        {
            format = ShaderFormat.DXIL;
        }

        if (result.objectBytes != null)
        {
            Logger.Info($"Generated {result.objectBytes.Length} bytes of shader code");
            return result.objectBytes;
        }
        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool GenerateAsSpirv(string backend)
    {
        return backend == "vulkan";
    }
}

public static class AssetStorageExt
{
    public static Shader CompileCrossVertex(this AssetStorage uploader, string filename, ShaderCreateInfo info)
    {
        return DXC.CompileVertex(uploader.GraphicsDevice, filename, info);
    }

    public static Shader CompileCrossFragment(this AssetStorage uploader, string filename, ShaderCreateInfo info)
    {
        return DXC.CompileFragment(uploader.GraphicsDevice, filename, info);
    }
}