struct VertexInput {
    @location(0) position: vec4<f32>,
    @location(1) texCoord: vec2<f32>,
    @location(2) color: vec4<f32>
};

struct VertexOutput {
    @builtin(position) position: vec4<f32>,
    @location(0) color: vec4<f32>,
    @location(1) texCoord: vec2<f32>
};

@group(1)
@binding(0)
var<uniform> MatrixUniform: mat4x4<f32>;

@vertex
fn main(in: VertexInput) -> VertexOutput {
    var output: VertexOutput;

    output.color = in.color;
    output.texCoord = in.texCoord;
    output.position = MatrixUniform * in.position;

    return output;
}