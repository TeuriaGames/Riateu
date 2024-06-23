#version 450
layout(location = 0) in vec3 position;
layout(location = 1) in vec4 color;

layout(location = 0) out vec4 outColor;

layout (set = 1, binding = 0) uniform UniformBlock
{
    mat4x4 MatrixUniform;
};

void main() {
    outColor = color;
    gl_Position = MatrixUniform * vec4(position, 1);
}
