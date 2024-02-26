#import "./Utils/MatrixHelper.wgsl"

struct VertexInput {
    @location(0) position: vec3<f32>,
}

struct InstanceInput {
    @location(1) pos: vec3<f32>,

    @location(2) uv0: vec2<f32>,
    @location(3) uv1: vec2<f32>,
    @location(4) uv2: vec2<f32>,
    @location(5) uv3: vec2<f32>,

    @location(6) scale: vec2<f32>,
    @location(7) origin: vec2<f32>,
    @location(8) rotation : f32,
    @location(9) color: vec4<f32>
};

struct VertexOutput {
    @builtin(position) position: vec4<f32>,
    @location(0) color: vec4<f32>,
    @location(1) tex_coord: vec2<f32>
};

@binding(0) @group(2) var<uniform> matrix_uniform: mat4x4<f32>;


@vertex
fn main(
    vert: VertexInput,
    in: InstanceInput,
    @builtin(vertex_index) vert_index: u32,
) -> VertexOutput {
    var output: VertexOutput;
    var uvs = array<vec2<f32>, 4>(in.uv0, in.uv1, in.uv2, in.uv3);
    let matrix = 
        create_translation_2d(-in.origin)
        * create_translation(in.pos)
        * create_rotation(in.rotation, in.origin)
        * create_scale(vec3<f32>(in.scale.xy, 1.));
    output.color = in.color;
    output.tex_coord = uvs[vert_index % 4u];
    output.position = matrix_uniform * matrix * vec4<f32>(vert.position, 1.0);

    return output;
} 
