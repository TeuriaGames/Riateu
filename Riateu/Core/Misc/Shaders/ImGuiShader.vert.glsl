#version 450
layout(location = 0) in vec2 position;
layout(location = 1) in vec2 tex_coord;
layout(location = 2) in vec4 color;

layout(location = 0) out vec4 outColor;
layout(location = 1) out vec2 outTexCoord;

layout (set = 1, binding = 0) uniform UniformBlock
{
    mat4x4 MatrixUniform;
};

void main() {
    outColor = color;
    outTexCoord = outTexCoord;
    gl_Position = MatrixUniform * vec4(position, 0, 1);
}
