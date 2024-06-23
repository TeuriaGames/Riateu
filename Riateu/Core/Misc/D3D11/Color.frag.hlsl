struct type_1 {
    float4 FragColor : SV_Target0;
};

static float4 color_1 = (float4)0;
static float4 FragColor = (float4)0;

struct FragmentInput_main {
    float4 color_2 : LOC0;
};

void main_1()
{
    float4 _expr2 = color_1;
    FragColor = _expr2;
    return;
}

type_1 Constructtype_1(float4 arg0) {
    type_1 ret = (type_1)0;
    ret.FragColor = arg0;
    return ret;
}

type_1 main(FragmentInput_main fragmentinput_main)
{
    float4 color = fragmentinput_main.color_2;
    color_1 = color;
    main_1();
    float4 _expr7 = FragColor;
    const type_1 type_1_ = Constructtype_1(_expr7);
    return type_1_;
}
