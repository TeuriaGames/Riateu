#import Utils::MatrixHelper
#import Structs::PositionVertex
#import Structs::InstancedVertex

struct VertexOutput {
    @builtin(position) position: vec4<f32>,
    @location(0) color: vec4<f32>,
    @location(1) tex_coord: vec2<f32>
};

@binding(0) @group(2) var<uniform> matrix_uniform: mat4x4<f32>;


@vertex
fn main(
    vert: PositionVertex,
    in: InstancedVertex,
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
