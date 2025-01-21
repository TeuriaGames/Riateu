struct VertexOutput
{
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD1;   
};

[vk_binding(0, 2)]
Sampler2D Texture: register(s0, space2);


[shader("fragment")]
float4 main(VertexOutput output) : SV_Target0
{
    float4 texture = Texture.Sample(output.TexCoord) * output.Color;

    if (texture.a == 0.) {
        discard;
    }

    return texture;
}
