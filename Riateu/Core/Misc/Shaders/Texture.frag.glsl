#version 450
layout(location = 0) in vec4 color;
layout(location = 1) in vec2 texCoord;

layout(location = 0) out vec4 FragColor;

layout(set = 2, binding = 0) uniform texture2D texture;
layout(set = 2, binding = 1) uniform sampler texSampler;

void main() {
    FragColor = texture(sampler2D(texture, texSampler), texCoord) * color;
}
