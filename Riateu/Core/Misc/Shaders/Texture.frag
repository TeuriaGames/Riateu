#version 450

layout (location = 0) in vec4 color;
layout (location = 1) in vec2 texCoord;

layout (location = 0) out vec4 FragColor;

layout (set = 2, binding = 0) uniform sampler2D texSampler;

void main() 
{
    vec4 texture = texture(texSampler, texCoord) * color;
    if (texture.a == 0.) {
        discard;
    }
    FragColor = texture;
}
