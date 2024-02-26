#import Structs::PositionTextureColorVertex

struct VertexOutput {
    @builtin(position) position: vec4<f32>,
    @location(0) color: vec4<f32>,
    @location(1) tex_coord: vec2<f32>
};

@binding(0) @group(2) var<uniform> matrix_uniform: mat4x4<f32>;

@vertex
fn main(in: PositionTextureColorVertex) -> VertexOutput {
    var output: VertexOutput;
    output.color = in.color;
    output.tex_coord = in.tex_coord;
    output.position = matrix_uniform * vec4<f32>(in.position, 1.0);
    return output;
}
