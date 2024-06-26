struct VertexInput {
    float4 color : LOC0;
    float2 texCoord : LOC1;
};

Texture2D<float4> texture_ : register(t0, space2);
SamplerState texSampler : register(s1, space2);

struct FragmentInput_main {
    float4 color : LOC0;
    float2 texCoord : LOC1;
};

float4 main(FragmentInput_main fragmentinput_main) : SV_Target0
{
    VertexInput in_ = { fragmentinput_main.color, fragmentinput_main.texCoord };
    float4 _expr4 = texture_.Sample(texSampler, in_.texCoord);
    return (_expr4 * in_.color);
}
