struct UniformBlock {
    row_major float4x4 MatrixUniform;
};

struct type_7 {
    float4 outColor : LOC0;
    float2 outTexCoord : LOC1;
    float4 gl_Position : SV_Position;
};

static float3 position_1 = (float3)0;
static float3 pos_5 = (float3)0;
static float2 uv_4[4] = (float2[4])0;
static float2 scale_1 = (float2)0;
static float2 origin_1 = (float2)0;
static float rotation_1 = (float)0;
static float4 color_1 = (float4)0;
static float4 outColor = (float4)0;
static float2 outTexCoord = (float2)0;
cbuffer global : register(b0, space1) { UniformBlock global; }
static uint gl_VertexIndex_1 = (uint)0;
static float4 gl_Position = (float4)0;

struct VertexOutput_main {
    float4 outColor_1 : LOC0;
    float2 outTexCoord_1 : LOC1;
    float4 gl_Position_1 : SV_Position;
};

float4x4 createScale(float3 scale2D)
{
    float3 scale2D_1 = (float3)0;

    scale2D_1 = scale2D;
    float3 _expr13 = scale2D_1;
    float3 _expr19 = scale2D_1;
    float3 _expr25 = scale2D_1;
    return float4x4(float4(_expr13.x, 0.0, 0.0, 0.0), float4(0.0, _expr19.y, 0.0, 0.0), float4(0.0, 0.0, _expr25.z, 0.0), float4(0.0, 0.0, 0.0, 1.0));
}

float4x4 createTranslation2D(float2 pos_1)
{
    float2 pos_2 = (float2)0;

    pos_2 = pos_1;
    float2 _expr25 = pos_2;
    float2 _expr27 = pos_2;
    return float4x4(float4(1.0, 0.0, 0.0, 0.0), float4(0.0, 1.0, 0.0, 0.0), float4(0.0, 0.0, 1.0, 0.0), float4(_expr25.x, _expr27.y, 0.0, 1.0));
}

float4x4 createTranslation(float3 pos_3)
{
    float3 pos_4 = (float3)0;

    pos_4 = pos_3;
    float3 _expr25 = pos_4;
    float3 _expr27 = pos_4;
    float3 _expr29 = pos_4;
    return float4x4(float4(1.0, 0.0, 0.0, 0.0), float4(0.0, 1.0, 0.0, 0.0), float4(0.0, 0.0, 1.0, 0.0), float4(_expr25.x, _expr27.y, _expr29.z, 1.0));
}

float4x4 createRotation(float rot, float2 centerPoint)
{
    float rot_1 = (float)0;
    float2 centerPoint_1 = (float2)0;
    float val1_ = (float)0;
    float val2_ = (float)0;
    float x = (float)0;
    float y = (float)0;

    rot_1 = rot;
    centerPoint_1 = centerPoint;
    float _expr16 = rot_1;
    val1_ = cos(_expr16);
    float _expr20 = rot_1;
    val2_ = sin(_expr20);
    float2 _expr23 = centerPoint_1;
    float _expr26 = val1_;
    float2 _expr29 = centerPoint_1;
    float _expr31 = val2_;
    x = ((_expr23.x * (1.0 - _expr26)) + (_expr29.y * _expr31));
    float2 _expr35 = centerPoint_1;
    float _expr38 = val1_;
    float2 _expr41 = centerPoint_1;
    float _expr43 = val2_;
    y = ((_expr35.y * (1.0 - _expr38)) - (_expr41.x * _expr43));
    float _expr47 = val1_;
    float _expr48 = val2_;
    float _expr51 = val2_;
    float _expr53 = val1_;
    float _expr56 = x;
    float _expr57 = y;
    return float4x4(float4(_expr47, _expr48, 0.0, 0.0), float4(-(_expr51), _expr53, 0.0, 0.0), float4(_expr56, _expr57, 1.0, 0.0), float4(0.0, 0.0, 0.0, 1.0));
}

void main_1()
{
    float4x4 matrix_ = (float4x4)0;

    float4 _expr11 = color_1;
    outColor = _expr11;
    uint _expr13 = gl_VertexIndex_1;
    float2 _expr18 = uv_4[(_expr13 % 4u)];
    outTexCoord = _expr18;
    float2 _expr19 = origin_1;
    float2 _expr21 = origin_1;
    const float4x4 _e23 = createTranslation2D(-(_expr21));
    float3 _expr25 = pos_5;
    const float4x4 _e26 = createTranslation(_expr25);
    float _expr30 = rotation_1;
    float2 _expr31 = origin_1;
    const float4x4 _e32 = createRotation(_expr30, _expr31);
    float2 _expr34 = scale_1;
    float2 _expr35 = _expr34.xy;
    float2 _expr40 = scale_1;
    float2 _expr41 = _expr40.xy;
    const float4x4 _e46 = createScale(float3(_expr41.x, _expr41.y, 1.0));
    matrix_ = mul(_e46, mul(_e32, mul(_e26, _e23)));
    float4x4 _expr50 = global.MatrixUniform;
    float4x4 _expr51 = matrix_;
    float3 _expr53 = position_1;
    gl_Position = mul(float4(_expr53.x, _expr53.y, _expr53.z, 1.0), mul(_expr51, _expr50));
    return;
}

type_7 Constructtype_7(float4 arg0, float2 arg1, float4 arg2) {
    type_7 ret = (type_7)0;
    ret.outColor = arg0;
    ret.outTexCoord = arg1;
    ret.gl_Position = arg2;
    return ret;
}

VertexOutput_main main(float3 position : LOC0, float3 pos : LOC1, float2 uv : LOC2, float2 uv_1 : LOC3, float2 uv_2 : LOC4, float2 uv_3 : LOC5, float2 scale : LOC6, float2 origin : LOC7, float rotation : LOC8, float4 color : LOC9, uint gl_VertexIndex : SV_VertexID)
{
    position_1 = position;
    pos_5 = pos;
    uv_4[0] = uv;
    uv_4[1] = uv_1;
    uv_4[2] = uv_2;
    uv_4[3] = uv_3;
    scale_1 = scale;
    origin_1 = origin;
    rotation_1 = rotation;
    color_1 = color;
    gl_VertexIndex_1 = gl_VertexIndex;
    main_1();
    float4 _expr44 = outColor;
    float2 _expr46 = outTexCoord;
    float4 _expr48 = gl_Position;
    const type_7 type_7_ = Constructtype_7(_expr44, _expr46, _expr48);
    const VertexOutput_main type_7_1 = { type_7_.outColor, type_7_.outTexCoord, type_7_.gl_Position };
    return type_7_1;
}
