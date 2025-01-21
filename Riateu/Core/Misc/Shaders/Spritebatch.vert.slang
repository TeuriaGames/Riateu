struct SpriteBatchData 
{
    float2 UV[4]; // 32

    float3 Position;
    float Rotation; // 48

    float4 Color; // 64

    float2 Scale;
    float2 Origin; // 80
};

struct VertexOutput 
{
    float4 Color : COLOR1;
    float2 TexCoord : TEXCOORD0;
    float4 Position : SV_Position;
};

[vk_binding(0, 0)]
StructuredBuffer<SpriteBatchData> BatchDataBuffer : register(t0, space0);

[vk_binding(0, 1)]
cbuffer UniformBlock : register(b0, space1) 
{
    float4x4 ViewProjectionMatrix : packoffset(c0);
};

static const float2 vertices[4] = {
    { 0.0f, 0.0f },
    { 1.0f, 0.0f },
    { 0.0f, 1.0f },
    { 1.0f, 1.0f },
};
static const uint indices[6] = { 0, 1, 2, 2, 1, 3 };

[shader("vertex")]
VertexOutput main(uint id: SV_VertexID) 
{
    uint spriteIndex = id / 6;
    SpriteBatchData sprite = BatchDataBuffer[spriteIndex];

    uint vert = indices[id - spriteIndex * 6];

    float c = cos(sprite.Rotation);
    float s = sin(sprite.Rotation);

    float2 coord = vertices[vert] * sprite.Scale;

    float2x2 rotation = { c, s, -s, c };
    coord -= sprite.Origin;
    coord = mul(coord, rotation);

    float3 coordDepth = float3(coord + sprite.Position.xy, sprite.Position.z);

    VertexOutput output;
    output.Position = mul(ViewProjectionMatrix, float4(coordDepth, 1.0f));
    output.TexCoord = sprite.UV[vert % 4];
    output.Color = sprite.Color;

    return output;
}