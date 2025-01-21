struct VertexOutput
{
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD1;
}

[vk_binding(0, 2)]
Sampler2D Texture : register(s0, space2);

[vk_binding(0, 1)]
cbuffer UniformBlock : register(b0, space3) 
{
    float pxRange : packoffset(c0);
};


float median(float r, float g, float b) 
{
    return max(min(r, g), min(max(r, g), b));
}

float screenPxRange(in float2 texCoord) 
{
    uint width, height, levels;
    Texture.GetDimensions(0, width, height, levels);
    float2 unitRange = pxRange / int2(width, height);
    float2 screenTexSize = 1.0 / fwidth(texCoord);
    return max(0.5 * dot(unitRange, screenTexSize), 1.0);
}

[shader("fragment")]
float4 main(VertexOutput output) : SV_Target0
{
    float3 msd = Texture.Sample(output.TexCoord).rgb;
    float sd = median(msd.r, msd.g, msd.b);
    float screenPxDistance = screenPxRange(output.TexCoord) * (sd - 0.5f);
    float opacity = clamp(screenPxDistance + 0.5, 0.0, 1.0);
    return lerp(float4(0.0, 0.0, 0.0, 0.0), output.Color, opacity);
}