// language: metal1.0
#include <metal_stdlib>
#include <simd/simd.h>

using metal::uint;

struct VertexInput {
    metal::float4 color;
    metal::float2 texCoord;
};

struct main_Input {
    metal::float4 color [[user(loc0), center_perspective]];
    metal::float2 texCoord [[user(loc1), center_perspective]];
};
struct main_Output {
    metal::float4 member [[color(0)]];
};
fragment main_Output main_(
  main_Input varyings [[stage_in]]
, metal::texture2d<float, metal::access::sample> texture [[user(fake0)]]
, metal::sampler texSampler [[user(fake0)]]
) {
    const VertexInput in = { varyings.color, varyings.texCoord };
    metal::float4 _e4 = texture.sample(texSampler, in.texCoord);
    return main_Output { _e4 * in.color };
}
