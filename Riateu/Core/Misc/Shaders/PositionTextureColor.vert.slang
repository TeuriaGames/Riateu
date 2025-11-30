import common.structs;

struct VertexOutput
{
    float4 Position : SV_Position;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD1;   
};

[vk_binding(0, 1)]
cbuffer UniformBlock : register(b0, space1) 
{
    float4x4 ViewProjectionMatrix : packoffset(c0);
};

[shader("vertex")]
VertexOutput main(PositionTextureColor input) 
{
    VertexOutput output;
    output.Position = mul(ViewProjectionMatrix, input.Position);
    output.Color = input.Color;
    output.TexCoord = input.TexCoord;
    return output;
}
