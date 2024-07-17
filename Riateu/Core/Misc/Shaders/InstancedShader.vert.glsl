#version 450
layout (location = 0) in vec3 position;

layout (location = 1) in vec3 pos;

layout (location = 2) in vec2 uv[4];

layout (location = 6) in vec2 scale;
layout (location = 7) in vec2 origin;
layout (location = 8) in float rotation;
layout (location = 9) in vec4 color;

layout (location = 0) out vec4 outColor;
layout (location = 1) out vec2 outTexCoord;

layout (set = 1, binding = 0) uniform UniformBlock
{
    mat4x4 MatrixUniform;
};

mat4 createScale(vec3 scale2D) {
    return mat4(
        scale2D.x, 0., 0., 0.,
        0., scale2D.y, 0., 0.,
        0., 0., scale2D.z, 0.,
        0., 0., 0., 1.
    );
}

mat4 createTranslation2D(vec2 pos) {
    return mat4(
        1., 0., 0., 0.,
        0., 1., 0., 0.,
        0., 0., 1., 0.,
        pos.x, pos.y, 0., 1.
    );
}

mat4 createTranslation(vec3 pos) {
    return mat4(
        1., 0., 0., 0.,
        0., 1., 0., 0.,
        0., 0., 1., 0.,
        pos.x, pos.y, pos.z, 1.
    );
}

mat4 createRotation(float rot, vec2 centerPoint) {
    float val1 = cos(rot);
    float val2 = sin(rot);

    float x = centerPoint.x * (1. - val1) + centerPoint.y * val2;
    float y = centerPoint.y * (1. - val1) - centerPoint.x * val2;

    return mat4(
        val1, val2, 0., 0.,
        -val2, val1, 0., 0.,
        x, y, 1., 0.,
        0., 0., 0., 1.
    );
}

void main() {
    outColor = color;
    outTexCoord = uv[gl_VertexIndex % 4];
    mat4 matrix =
        createTranslation2D(-origin)
        * createTranslation(pos)
        * createRotation(rotation, origin)
        * createScale(vec3(scale.xy, 1.));
    gl_Position = MatrixUniform * matrix * vec4(position, 1.0);
}
