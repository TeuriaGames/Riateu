struct ParitcleData
{
    float2 Position;
    float2 Velocity;
    float StartLife;
    float LifeTime;
    float Rotation;
    float Emitting;
    float4 StartColor;
    float4 Color;
};

[vk_binding(0, 0)]
StructuredBuffer<ParitcleData> Particles : register(t0, space0);

[vk_binding(0, 1)]
cbuffer UniformBlock : register(b0, space1) 
{
    float2 Acceleration : packoffset(c0);
    float Friction : packoffset(c1);
    float Spin : packoffset(c2);
    float Delta : packoffset(c3);
};


float2 moveTowards(float2 current, float2 target, float maxDelta)
{
    float2 velocityDistance = target - current;
    float len = length(velocityDistance);
    return current + (velocityDistance / len * maxDelta);
}

[shader("compute")]
[numthreads(64, 1, 1)]
void main(uint3 id : SV_DispatchThreadID) 
{
    ParitcleData particle = Particles[id.x];

    float ease = particle.LifeTime / particle.StartLife;
    particle.LifeTime -= Delta;
    particle.Emitting = max(0, particle.LifeTime);
    particle.Position += particle.Velocity * Delta;
    particle.Velocity += Acceleration * Delta;
    particle.Velocity = moveTowards(particle.Velocity, float2(0, 0), Friction * Delta);

    particle.Rotation += Spin * Delta;

    float alpha = ease;

    particle.Color *= max(0, alpha);
}