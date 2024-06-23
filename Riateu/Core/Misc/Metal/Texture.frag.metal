// language: metal1.0
#include <metal_stdlib>
#include <simd/simd.h>

using metal::uint;

struct type_28 {
    metal::float4 FragColor;
};

void main_1(
    thread metal::float4& color_1,
    thread metal::float2& texCoord_1,
    thread metal::float4& FragColor,
    metal::texture2d<float, metal::access::sample> texture,
    metal::sampler texSampler
) {
    metal::float2 _e6 = texCoord_1;
    metal::float4 _e7 = texture.sample(texSampler, _e6);
    metal::float4 _e8 = color_1;
    FragColor = _e7 * _e8;
    return;
}

struct main_Input {
    metal::float4 color [[user(loc0), center_perspective]];
    metal::float2 texCoord [[user(loc1), center_perspective]];
};
struct main_Output {
    metal::float4 FragColor [[color(0)]];
};
fragment main_Output main_(
  main_Input varyings [[stage_in]]
, metal::texture2d<float, metal::access::sample> texture [[user(fake0)]]
, metal::sampler texSampler [[user(fake0)]]
) {
    metal::float4 color_1 = {};
    metal::float2 texCoord_1 = {};
    metal::float4 FragColor = {};
    const auto color = varyings.color;
    const auto texCoord = varyings.texCoord;
    color_1 = color;
    texCoord_1 = texCoord;
    main_1(color_1, texCoord_1, FragColor, texture, texSampler);
    metal::float4 _e15 = FragColor;
    const auto _tmp = type_28 {_e15};
    return main_Output { _tmp.FragColor };
}
