#version 450

layout(location = 0) in vec4 position;
layout(location = 1) in vec2 texCoord;
layout(location = 2) in vec4 color;

layout(location = 0) out vec4 outColor;
layout(location = 1) out vec2 outTexCoord;

layout (set = 1, binding = 0) uniform UniformBlock
{
    mat4x4 MatrixUniform;
};

void main() 
{
    outColor = color;    
    outTexCoord = texCoord;
    gl_Position = MatrixUniform * position;
}
