using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Riateu.Audios;
using Riateu.Graphics;

namespace Riateu.Content;

public interface IAssets {}

public class Ref<T>(T data)
{
    public T Data = data;

    public static implicit operator T(Ref<T> refs) 
    {
        return refs.Data;
    }
}

public record struct AtlasItem(string Name, Image Image);

public class AssetStorage
{
    private Dictionary<string, IAssets> assetsCache = new Dictionary<string, IAssets>();
    private string assetPath;
    private ResourceUploader uploader;
    private Packer<AtlasItem> packer;
    private AudioDevice device;

    public AssetStorage(AudioDevice device, string path) 
    {
        assetPath = path;
        packer = new Packer<AtlasItem>(8192);
        this.device = device;
    }

    public void StartContext(ResourceUploader uploader) 
    {
        this.uploader = uploader;
    }

    /// <summary>
    /// Create an <see cref="Riateu.Graphics.Atlas"/> from a folder. This will look all of PNGs file recursively in this folder.
    /// </summary>
    /// <param name="basePath">A target folder to look for</param>
    /// <returns>An <see cref="Riateu.Graphics.Atlas"/></returns>
    public Atlas CreateAtlas(string basePath) 
    {
        void Crawl(string path) 
        {
            var directories = Directory.GetDirectories(path);
            foreach (var directory in directories) 
            {
                Crawl(directory);
            }

            var files = Directory.GetFiles(path).Where(x => x.EndsWith("png"));
            foreach (var file in files) 
            {
                string name = Path.Join(path, Path.GetFileNameWithoutExtension(file)).Replace('\\', '/')
                    .Substring(basePath.Length + 1);

                Image image = new Image(file);
                packer.Add(new Packer<AtlasItem>.Item(new AtlasItem(name, image), image.Width, image.Height));
            }
        }
        if (basePath.EndsWith(Path.DirectorySeparatorChar)) 
        {
            basePath = basePath.Substring(0, basePath.Length - 1);
        }

        Crawl(basePath);


        if (packer.Pack(out List<Packer<AtlasItem>.PackedItem> items, out Point size)) 
        {
            return Atlas.LoadFromPacker(uploader, items, size);
        }
        return null;
    }

    /// <summary>
    /// Create an <see cref="Riateu.Graphics.Atlas"/> from a packer.
    /// </summary>
    /// <param name="packer">A packer that contains all of the added images</param>
    /// <returns>An <see cref="Riateu.Graphics.Atlas"/></returns>
    public Atlas CreateAtlasFromPacker(Packer<AtlasItem> packer) 
    {
        if (packer.Pack(out List<Packer<AtlasItem>.PackedItem> items, out Point size)) 
        {
            return Atlas.LoadFromPacker(uploader, items, size);
        }
        return null;
    }

    public Ref<Texture> LoadTexture(string path) 
    {
        if (assetsCache.TryGetValue(path, out IAssets asset)) 
        {
            return (Ref<Texture>)asset;
        }
        return new Ref<Texture>(uploader.CreateTexture2DFromCompressed(path));
    }

    public AudioTrack LoadAudioTrack(string path, TrackFormat format) 
    {
        switch (format) 
        {
        case TrackFormat.OGG:
            return AudioTrackOGG.CreateOGG(device, path);
        case TrackFormat.WAV:
            throw new NotImplementedException("WAV format has not been implemented yet");
        default:
            throw new InvalidOperationException("Unknown format is passed.");
        }
    }

    public Atlas LoadAtlas(string path, Texture texture, bool ninePatchEnabled = false, JsonType fileType = JsonType.Json) 
    {
        if (assetsCache.TryGetValue(path, out IAssets asset)) 
        {
            return (Atlas)asset;
        }
        Atlas atlas = Atlas.LoadFromFile(path, texture, fileType, ninePatchEnabled);
        assetsCache.Add(path, atlas);

        return atlas;
    }

    public SpriteFont LoadFont(string path, Texture texture) 
    {
        if (assetsCache.TryGetValue(path, out IAssets asset)) 
        {
            return (SpriteFont)asset;
        }

        SpriteFont spriteFont = new SpriteFont(texture, path);
        assetsCache.Add(path, spriteFont);
        return spriteFont;
    }

    public SpriteFont LoadFont(string path, Texture texture, TextureQuad quad) 
    {
        if (assetsCache.TryGetValue(path, out IAssets asset)) 
        {
            return (SpriteFont)asset;
        }

        SpriteFont spriteFont = new SpriteFont(texture, quad, path);
        assetsCache.Add(path, spriteFont);
        return spriteFont;
    }

    public AnimationStorage LoadAnimations(string path, Atlas atlas, JsonType fileType = JsonType.Json) 
    {
        if (assetsCache.TryGetValue(path, out IAssets asset)) 
        {
            return (AnimationStorage)asset;
        }

        AnimationStorage storage = AnimationStorage.Create(path, atlas, fileType);
        assetsCache.Add(path, storage);
        return storage;
    }

    public void EndContext() 
    {
        uploader.Upload();
        uploader.Dispose();
    }
}