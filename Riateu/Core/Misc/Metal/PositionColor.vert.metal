// language: metal1.0
#include <metal_stdlib>
#include <simd/simd.h>

using metal::uint;

struct UniformBlock {
    metal::float4x4 MatrixUniform;
};
struct type_3 {
    metal::float4 outColor;
    metal::float4 gl_Position;
};

void main_1(
    thread metal::float3& position_1,
    thread metal::float4& color_1,
    thread metal::float4& outColor,
    constant UniformBlock& global,
    thread metal::float4& gl_Position
) {
    metal::float4 _e5 = color_1;
    outColor = _e5;
    metal::float4x4 _e7 = global.MatrixUniform;
    metal::float3 _e8 = position_1;
    gl_Position = _e7 * metal::float4(_e8.x, _e8.y, _e8.z, 1.0);
    return;
}

struct main_Input {
    metal::float3 position [[attribute(0)]];
    metal::float4 color [[attribute(1)]];
};
struct main_Output {
    metal::float4 outColor [[user(loc0), center_perspective]];
    metal::float4 gl_Position [[position]];
};
vertex main_Output main_(
  main_Input varyings [[stage_in]]
, constant UniformBlock& global [[user(fake0)]]
) {
    metal::float3 position_1 = {};
    metal::float4 color_1 = {};
    metal::float4 outColor = {};
    metal::float4 gl_Position = {};
    const auto position = varyings.position;
    const auto color = varyings.color;
    position_1 = position;
    color_1 = color;
    main_1(position_1, color_1, outColor, global, gl_Position);
    metal::float4 _e13 = outColor;
    metal::float4 _e15 = gl_Position;
    const auto _tmp = type_3 {_e13, _e15};
    return main_Output { _tmp.outColor, _tmp.gl_Position };
}
