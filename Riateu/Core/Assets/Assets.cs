using System.Collections.Generic;
using MoonWorks.Graphics;
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

public class AssetStorage
{
    private Dictionary<string, IAssets> assetsCache = new Dictionary<string, IAssets>();
    private string assetPath;
    private ResourceUploader uploader;

    public AssetStorage(string path) 
    {
        assetPath = path;
    }

    public void StartContext(ResourceUploader uploader) 
    {
        this.uploader = uploader;
    }

    public Ref<Texture> LoadTexture(string path) 
    {
        if (assetsCache.TryGetValue(path, out IAssets asset)) 
        {
            return (Ref<Texture>)asset;
        }
        return new Ref<Texture>(uploader.CreateTexture2DFromCompressed(path));
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

    public SpriteFont LoadFont(string path, Texture texture, Quad quad) 
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