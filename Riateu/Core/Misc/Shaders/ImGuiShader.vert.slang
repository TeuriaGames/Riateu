struct VertexInput 
{
    float2 Position : POSITION0;
    float2 TexCoord : TEXCOORD1;
    float4 Color : COLOR2;
};

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
VertexOutput main(VertexInput input) 
{
    VertexOutput output;
    output.Position = mul(ViewProjectionMatrix, float4(input.Position.xy, 0., 1.));
    output.Color = input.Color;
    output.TexCoord = input.TexCoord;
    return output;
}