struct VertexInput {
    @location(0) position: vec3<f32>,
    @location(1) tex_coord: vec2<f32>,
    @location(2) color: vec4<f32>,
};

struct VertexOutput {
    @builtin(position) position: vec4<f32>,
    @location(0) color: vec4<f32>,
    @location(1) tex_coord: vec2<f32>
};

@binding(0) @group(2) var<uniform> matrix_uniform: mat4x4<f32>;

@vertex
fn vs_main(in: VertexInput) -> VertexOutput {
    var output: VertexOutput;
    output.color = in.color;
    output.tex_coord = in.tex_coord;
    output.position = matrix_uniform * vec4<f32>(in.position, 1.0);
    return output;
}
