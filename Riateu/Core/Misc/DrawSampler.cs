using Riateu.Graphics;

namespace Riateu;


public static class DrawSampler 
{
    public static Sampler PointClamp 
    { 
        get 
        {
            if (pointClamp == null)
                pointClamp = new Sampler(GameContext.GraphicsDevice, SamplerCreateInfo.PointClamp);
            return pointClamp;
        }
    }
    private static Sampler pointClamp;
    public static Sampler PointWrap 
    { 
        get 
        {
            if (pointWrap == null)
                pointWrap = new Sampler(GameContext.GraphicsDevice, SamplerCreateInfo.PointWrap);
            return pointWrap;
        }
    }
    private static Sampler pointWrap;
    public static Sampler LinearClamp 
    { 
        get 
        {
            if (linearClamp == null)
                linearClamp = new Sampler(GameContext.GraphicsDevice, SamplerCreateInfo.LinearClamp);
            return linearClamp;
        }
    }
    private static Sampler linearClamp;
    public static Sampler LinearWrap
    { 
        get 
        {
            if (linearWrap == null)
                linearWrap = new Sampler(GameContext.GraphicsDevice, SamplerCreateInfo.LinearWrap);
            return linearWrap;
        }
    }
    private static Sampler linearWrap;
    public static Sampler AnisotropicClamp
    { 
        get 
        {
            if (anisotropicClamp == null)
                anisotropicClamp = new Sampler(GameContext.GraphicsDevice, SamplerCreateInfo.AnisotropicClamp);
            return anisotropicClamp;
        }
    }
    private static Sampler anisotropicClamp;
    public static Sampler AnisotropicWrap
    { 
        get 
        {
            if (anisotropicWrap == null)
                anisotropicWrap = new Sampler(GameContext.GraphicsDevice, SamplerCreateInfo.AnisotropicWrap);
            return anisotropicWrap;
        }
    }
    private static Sampler anisotropicWrap;
}