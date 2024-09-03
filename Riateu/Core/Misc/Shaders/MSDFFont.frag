#version 450

layout(location = 0) in vec4 inColor;
layout(location = 1) in vec2 inTexCoord;

layout(location = 0) out vec4 outColor;

layout(set = 2, binding = 0) uniform sampler2D msdfTexture;

layout(set = 3, binding = 0) uniform UniformBlock
{
    float pxRange;
};

float median(float r, float g, float b) 
{
    return max(min(r, g), min(max(r, g), b));
}

float screenPxRange() 
{
    vec2 unitRange = vec2(pxRange) / vec2(textureSize(msdfTexture, 0));
    vec2 screenTexSize = vec2(1.0) / fwidth(inTexCoord);
    return max(0.5 * dot(unitRange, screenTexSize), 1.0);
}

void main() 
{
    vec3 msd = texture(msdfTexture, inTexCoord).rgb;
    float sd = median(msd.r, msd.g, msd.b);
    float screenPxDistance = screenPxRange() * (sd - 0.5f);
    float opacity = clamp(screenPxDistance + 0.5, 0.0, 1.0);
    outColor = mix(vec4(0.0, 0.0, 0.0, 0.0), inColor, opacity);
}