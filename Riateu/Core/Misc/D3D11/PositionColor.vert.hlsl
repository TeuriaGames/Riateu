struct UniformBlock {
    row_major float4x4 MatrixUniform;
};

struct type_3 {
    float4 outColor : LOC0;
    float4 gl_Position : SV_Position;
};

static float3 position_1 = (float3)0;
static float4 color_1 = (float4)0;
static float4 outColor = (float4)0;
cbuffer global : register(b0, space1) { UniformBlock global; }
static float4 gl_Position = (float4)0;

struct VertexOutput_main {
    float4 outColor_1 : LOC0;
    float4 gl_Position_1 : SV_Position;
};

void main_1()
{
    float4 _expr5 = color_1;
    outColor = _expr5;
    float4x4 _expr7 = global.MatrixUniform;
    float3 _expr8 = position_1;
    gl_Position = mul(float4(_expr8.x, _expr8.y, _expr8.z, 1.0), _expr7);
    return;
}

type_3 Constructtype_3(float4 arg0, float4 arg1) {
    type_3 ret = (type_3)0;
    ret.outColor = arg0;
    ret.gl_Position = arg1;
    return ret;
}

VertexOutput_main main(float3 position : LOC0, float4 color : LOC1)
{
    position_1 = position;
    color_1 = color;
    main_1();
    float4 _expr13 = outColor;
    float4 _expr15 = gl_Position;
    const type_3 type_3_ = Constructtype_3(_expr13, _expr15);
    const VertexOutput_main type_3_1 = { type_3_.outColor, type_3_.gl_Position };
    return type_3_1;
}
