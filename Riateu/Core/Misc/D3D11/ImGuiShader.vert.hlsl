struct UniformBlock {
    row_major float4x4 MatrixUniform;
};

struct type_3 {
    float4 outColor : LOC0;
    float2 outTexCoord : LOC1;
    float4 gl_Position : SV_Position;
};

static float2 position_1 = (float2)0;
static float2 tex_coord_1 = (float2)0;
static float4 color_1 = (float4)0;
static float4 outColor = (float4)0;
static float2 outTexCoord = (float2)0;
cbuffer global : register(b0, space1) { UniformBlock global; }
static float4 gl_Position = (float4)0;

struct VertexOutput_main {
    float4 outColor_1 : LOC0;
    float2 outTexCoord_1 : LOC1;
    float4 gl_Position_1 : SV_Position;
};

void main_1()
{
    float4 _expr7 = color_1;
    outColor = _expr7;
    float2 _expr8 = outTexCoord;
    outTexCoord = _expr8;
    float4x4 _expr10 = global.MatrixUniform;
    float2 _expr11 = position_1;
    gl_Position = mul(float4(_expr11.x, _expr11.y, 0.0, 1.0), _expr10);
    return;
}

type_3 Constructtype_3(float4 arg0, float2 arg1, float4 arg2) {
    type_3 ret = (type_3)0;
    ret.outColor = arg0;
    ret.outTexCoord = arg1;
    ret.gl_Position = arg2;
    return ret;
}

VertexOutput_main main(float2 position : LOC0, float2 tex_coord : LOC1, float4 color : LOC2)
{
    position_1 = position;
    tex_coord_1 = tex_coord;
    color_1 = color;
    main_1();
    float4 _expr19 = outColor;
    float2 _expr21 = outTexCoord;
    float4 _expr23 = gl_Position;
    const type_3 type_3_ = Constructtype_3(_expr19, _expr21, _expr23);
    const VertexOutput_main type_3_1 = { type_3_.outColor, type_3_.outTexCoord, type_3_.gl_Position };
    return type_3_1;
}
