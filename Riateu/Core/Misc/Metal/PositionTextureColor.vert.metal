// language: metal1.0
#include <metal_stdlib>
#include <simd/simd.h>

using metal::uint;

struct VertexInput {
    metal::float4 position;
    metal::float2 texCoord;
    char _pad2[8];
    metal::float4 color;
};
struct VertexOutput {
    metal::float4 position;
    metal::float4 color;
    metal::float2 texCoord;
};

struct main_Input {
    metal::float4 position [[attribute(0)]];
    metal::float2 texCoord [[attribute(1)]];
    metal::float4 color [[attribute(2)]];
};
struct main_Output {
    metal::float4 position [[position]];
    metal::float4 color [[user(loc0), center_perspective]];
    metal::float2 texCoord [[user(loc1), center_perspective]];
};
vertex main_Output main_(
  main_Input varyings [[stage_in]]
, constant metal::float4x4& MatrixUniform [[user(fake0)]]
) {
    const VertexInput in = { varyings.position, varyings.texCoord, {}, varyings.color };
    VertexOutput output = {};
    output.color = in.color;
    output.texCoord = in.texCoord;
    metal::float4x4 _e8 = MatrixUniform;
    output.position = _e8 * in.position;
    VertexOutput _e11 = output;
    const auto _tmp = _e11;
    return main_Output { _tmp.position, _tmp.color, _tmp.texCoord };
}
