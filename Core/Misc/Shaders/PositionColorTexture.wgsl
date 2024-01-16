struct VertexInput {
    @location(0) position: vec3<f32>,
    @location(1) color: vec4<f32>,
    @location(2) tex_coord: vec2<f32>
};

struct VertexOutput {
    @builtin(position) position: vec4<f32>,
    @location(0) color: vec4<f32>,
    @location(1) tex_coord: vec2<f32>
};

struct UniformBlock {
    matrix_uniform: mat4x4<f32>,
};

@binding(0) @group(2) var<uniform> uniform_block: UniformBlock;

@vertex
fn vs_main(in: VertexInput) -> VertexOutput {
    var output: VertexOutput;
    output.color = in.color;
    output.tex_coord = in.tex_coord;
    output.position = uniform_block.matrix_uniform * vec4<f32>(in.position, 1.0);
    return output;
}

@binding(0) @group(1) var tex_2d: texture_2d<f32>;
@binding(1) @group(1) var tex_sampler: sampler;


@fragment
fn fs_main(in: VertexOutput) -> @location(0) vec4<f32> {
    return textureSample(tex_2d, tex_sampler, in.tex_coord) * in.color;
}