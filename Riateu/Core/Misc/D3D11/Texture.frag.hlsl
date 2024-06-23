struct type_28 {
    float4 FragColor : SV_Target0;
};

static float4 color_1 = (float4)0;
static float2 texCoord_1 = (float2)0;
static float4 FragColor = (float4)0;
Texture2D<float4> texture_ : register(t0, space2);
SamplerState texSampler : register(s1, space2);

struct FragmentInput_main {
    float4 color_2 : LOC0;
    float2 texCoord_2 : LOC1;
};

void main_1()
{
    float2 _expr6 = texCoord_1;
    float4 _expr7 = texture_.Sample(texSampler, _expr6);
    float4 _expr8 = color_1;
    FragColor = (_expr7 * _expr8);
    return;
}

type_28 Constructtype_28(float4 arg0) {
    type_28 ret = (type_28)0;
    ret.FragColor = arg0;
    return ret;
}

type_28 main(FragmentInput_main fragmentinput_main)
{
    float4 color = fragmentinput_main.color_2;
    float2 texCoord = fragmentinput_main.texCoord_2;
    color_1 = color;
    texCoord_1 = texCoord;
    main_1();
    float4 _expr15 = FragColor;
    const type_28 type_28_ = Constructtype_28(_expr15);
    return type_28_;
}
