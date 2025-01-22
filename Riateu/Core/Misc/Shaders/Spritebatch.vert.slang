struct SpriteBatchData 
{
    float2 UV[2]; 

    float3 Position;
    float Rotation;

    float4 Color; 

    float2 Scale;
    float2 Origin;
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

static const uint2 vertices[4] = {
    { 0, 0 },
    { 1, 0 },
    { 0, 1 },
    { 1, 1 },
};

[shader("vertex")]
VertexOutput main(uint id: SV_VertexID, uint instanceID: SV_InstanceID) 
{
    uint spriteIndex = id / 4;
    SpriteBatchData sprite = BatchDataBuffer[instanceID];

    uint vert = id - spriteIndex * 4;

    float c = cos(sprite.Rotation);
    float s = sin(sprite.Rotation);

    uint2 vertIndex = vertices[vert];
    float2 coord = vertIndex * sprite.Scale;

    float2x2 rotation = { c, s, -s, c };
    coord -= sprite.Origin;
    coord = mul(coord, rotation);

    float3 coordDepth = float3(coord + sprite.Position.xy, sprite.Position.z);

    VertexOutput output;
    output.Position = mul(ViewProjectionMatrix, float4(coordDepth, 1.0f));
    output.TexCoord = float2(sprite.UV[vertIndex.x].x, sprite.UV[vertIndex.y].y);
    output.Color = sprite.Color;

    return output;
}