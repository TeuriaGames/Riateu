// language: metal1.0
#include <metal_stdlib>
#include <simd/simd.h>

using metal::uint;

struct type_1 {
    metal::float4 FragColor;
};

void main_1(
    thread metal::float4& color_1,
    thread metal::float4& FragColor
) {
    metal::float4 _e2 = color_1;
    FragColor = _e2;
    return;
}

struct main_Input {
    metal::float4 color [[user(loc0), center_perspective]];
};
struct main_Output {
    metal::float4 FragColor [[color(0)]];
};
fragment main_Output main_(
  main_Input varyings [[stage_in]]
) {
    metal::float4 color_1 = {};
    metal::float4 FragColor = {};
    const auto color = varyings.color;
    color_1 = color;
    main_1(color_1, FragColor);
    metal::float4 _e7 = FragColor;
    const auto _tmp = type_1 {_e7};
    return main_Output { _tmp.FragColor };
}
