using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Riateu.Graphics;

[Experimental("RIE005")]
public class ParticleEmitter 
{
    public ParticleUniform Uniform;

    public void UpdateParticle(Particle particle) 
    {
        var ease = particle.LifeTime / particle.StartLife;
        particle.LifeTime -= Uniform.Delta;
        if (particle.LifeTime < 0f) 
        {
            particle.Emitting = 0;
        }
        particle.Position += particle.Velocity * Uniform.Delta;
        particle.Velocity += Uniform.Acceleration * Uniform.Delta;
        particle.Velocity = MathUtils.MoveTowards(particle.Velocity, Vector2.Zero, Uniform.Friction * Uniform.Delta);
        particle.Rotation += Uniform.Spin * Uniform.Delta;

        float alpha;
        alpha = ease;

        if (alpha == 0)
            particle.Color = new Vector4(0, 0, 0, 0);
        else if (alpha < 1f)
            particle.Color *= alpha;
    }
}

[Experimental("RIE005")]
[StructLayout(LayoutKind.Explicit, Size = 64)]
public struct Particle 
{
    [FieldOffset(0)]
    public Vector2 Position;
    [FieldOffset(8)]
    public Vector2 Velocity;
    [FieldOffset(16)]
    public float StartLife;
    [FieldOffset(20)]
    public float LifeTime;
    [FieldOffset(24)]
    public float Rotation;
    [FieldOffset(28)]
    public float Emitting;
    [FieldOffset(32)]
    public Vector4 StartColor;
    [FieldOffset(48)]
    public Vector4 Color;
}

[Experimental("RIE005")]
public struct ParticleUniform 
{
    public Vector2 Acceleration;
    public float Friction;
    public float Spin;
    public float Delta;
}