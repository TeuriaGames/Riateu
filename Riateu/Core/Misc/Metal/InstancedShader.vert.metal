// language: metal1.0
#include <metal_stdlib>
#include <simd/simd.h>

using metal::uint;

struct type_2 {
    metal::float2 inner[4];
};
struct UniformBlock {
    metal::float4x4 MatrixUniform;
};
struct type_7 {
    metal::float4 outColor;
    metal::float2 outTexCoord;
    metal::float4 gl_Position;
};

metal::float4x4 createScale(
    metal::float3 scale2D
) {
    metal::float3 scale2D_1 = {};
    scale2D_1 = scale2D;
    metal::float3 _e13 = scale2D_1;
    metal::float3 _e19 = scale2D_1;
    metal::float3 _e25 = scale2D_1;
    return metal::float4x4(metal::float4(_e13.x, 0.0, 0.0, 0.0), metal::float4(0.0, _e19.y, 0.0, 0.0), metal::float4(0.0, 0.0, _e25.z, 0.0), metal::float4(0.0, 0.0, 0.0, 1.0));
}

metal::float4x4 createTranslation2D(
    metal::float2 pos_1
) {
    metal::float2 pos_2 = {};
    pos_2 = pos_1;
    metal::float2 _e25 = pos_2;
    metal::float2 _e27 = pos_2;
    return metal::float4x4(metal::float4(1.0, 0.0, 0.0, 0.0), metal::float4(0.0, 1.0, 0.0, 0.0), metal::float4(0.0, 0.0, 1.0, 0.0), metal::float4(_e25.x, _e27.y, 0.0, 1.0));
}

metal::float4x4 createTranslation(
    metal::float3 pos_3
) {
    metal::float3 pos_4 = {};
    pos_4 = pos_3;
    metal::float3 _e25 = pos_4;
    metal::float3 _e27 = pos_4;
    metal::float3 _e29 = pos_4;
    return metal::float4x4(metal::float4(1.0, 0.0, 0.0, 0.0), metal::float4(0.0, 1.0, 0.0, 0.0), metal::float4(0.0, 0.0, 1.0, 0.0), metal::float4(_e25.x, _e27.y, _e29.z, 1.0));
}

metal::float4x4 createRotation(
    float rot,
    metal::float2 centerPoint
) {
    float rot_1 = {};
    metal::float2 centerPoint_1 = {};
    float val1_ = {};
    float val2_ = {};
    float x = {};
    float y = {};
    rot_1 = rot;
    centerPoint_1 = centerPoint;
    float _e16 = rot_1;
    val1_ = metal::cos(_e16);
    float _e20 = rot_1;
    val2_ = metal::sin(_e20);
    metal::float2 _e23 = centerPoint_1;
    float _e26 = val1_;
    metal::float2 _e29 = centerPoint_1;
    float _e31 = val2_;
    x = (_e23.x * (1.0 - _e26)) + (_e29.y * _e31);
    metal::float2 _e35 = centerPoint_1;
    float _e38 = val1_;
    metal::float2 _e41 = centerPoint_1;
    float _e43 = val2_;
    y = (_e35.y * (1.0 - _e38)) - (_e41.x * _e43);
    float _e47 = val1_;
    float _e48 = val2_;
    float _e51 = val2_;
    float _e53 = val1_;
    float _e56 = x;
    float _e57 = y;
    return metal::float4x4(metal::float4(_e47, _e48, 0.0, 0.0), metal::float4(-(_e51), _e53, 0.0, 0.0), metal::float4(_e56, _e57, 1.0, 0.0), metal::float4(0.0, 0.0, 0.0, 1.0));
}

void main_1(
    thread metal::float3& position_1,
    thread metal::float3& pos_5,
    thread type_2& uv_4,
    thread metal::float2& scale_1,
    thread metal::float2& origin_1,
    thread float& rotation_1,
    thread metal::float4& color_1,
    thread metal::float4& outColor,
    thread metal::float2& outTexCoord,
    constant UniformBlock& global,
    thread uint& gl_VertexIndex_1,
    thread metal::float4& gl_Position
) {
    metal::float4x4 matrix_ = {};
    metal::float4 _e11 = color_1;
    outColor = _e11;
    uint _e13 = gl_VertexIndex_1;
    metal::float2 _e18 = uv_4.inner[_e13 % 4u];
    outTexCoord = _e18;
    metal::float2 _e19 = origin_1;
    metal::float2 _e21 = origin_1;
    metal::float4x4 _e23 = createTranslation2D(-(_e21));
    metal::float3 _e25 = pos_5;
    metal::float4x4 _e26 = createTranslation(_e25);
    float _e30 = rotation_1;
    metal::float2 _e31 = origin_1;
    metal::float4x4 _e32 = createRotation(_e30, _e31);
    metal::float2 _e34 = scale_1;
    metal::float2 _e35 = _e34.xy;
    metal::float2 _e40 = scale_1;
    metal::float2 _e41 = _e40.xy;
    metal::float4x4 _e46 = createScale(metal::float3(_e41.x, _e41.y, 1.0));
    matrix_ = ((_e23 * _e26) * _e32) * _e46;
    metal::float4x4 _e50 = global.MatrixUniform;
    metal::float4x4 _e51 = matrix_;
    metal::float3 _e53 = position_1;
    gl_Position = (_e50 * _e51) * metal::float4(_e53.x, _e53.y, _e53.z, 1.0);
    return;
}

struct main_Input {
    metal::float3 position [[attribute(0)]];
    metal::float3 pos [[attribute(1)]];
    metal::float2 uv [[attribute(2)]];
    metal::float2 uv_1 [[attribute(3)]];
    metal::float2 uv_2 [[attribute(4)]];
    metal::float2 uv_3 [[attribute(5)]];
    metal::float2 scale [[attribute(6)]];
    metal::float2 origin [[attribute(7)]];
    float rotation [[attribute(8)]];
    metal::float4 color [[attribute(9)]];
};
struct main_Output {
    metal::float4 outColor [[user(loc0), center_perspective]];
    metal::float2 outTexCoord [[user(loc1), center_perspective]];
    metal::float4 gl_Position [[position]];
};
vertex main_Output main_(
  main_Input varyings [[stage_in]]
, uint gl_VertexIndex [[vertex_id]]
, constant UniformBlock& global [[user(fake0)]]
) {
    metal::float3 position_1 = {};
    metal::float3 pos_5 = {};
    type_2 uv_4 = {};
    metal::float2 scale_1 = {};
    metal::float2 origin_1 = {};
    float rotation_1 = {};
    metal::float4 color_1 = {};
    metal::float4 outColor = {};
    metal::float2 outTexCoord = {};
    uint gl_VertexIndex_1 = {};
    metal::float4 gl_Position = {};
    const auto position = varyings.position;
    const auto pos = varyings.pos;
    const auto uv = varyings.uv;
    const auto uv_1 = varyings.uv_1;
    const auto uv_2 = varyings.uv_2;
    const auto uv_3 = varyings.uv_3;
    const auto scale = varyings.scale;
    const auto origin = varyings.origin;
    const auto rotation = varyings.rotation;
    const auto color = varyings.color;
    position_1 = position;
    pos_5 = pos;
    uv_4.inner[0] = uv;
    uv_4.inner[1] = uv_1;
    uv_4.inner[2] = uv_2;
    uv_4.inner[3] = uv_3;
    scale_1 = scale;
    origin_1 = origin;
    rotation_1 = rotation;
    color_1 = color;
    gl_VertexIndex_1 = gl_VertexIndex;
    main_1(position_1, pos_5, uv_4, scale_1, origin_1, rotation_1, color_1, outColor, outTexCoord, global, gl_VertexIndex_1, gl_Position);
    metal::float4 _e44 = outColor;
    metal::float2 _e46 = outTexCoord;
    metal::float4 _e48 = gl_Position;
    const auto _tmp = type_7 {_e44, _e46, _e48};
    return main_Output { _tmp.outColor, _tmp.outTexCoord, _tmp.gl_Position };
}
