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
    @location(7) color: vec4<f32>
};

struct VertexOutput {
    @builtin(position) position: vec4<f32>,
    @location(0) color: vec4<f32>,
    @location(1) tex_coord: vec2<f32>
};

@binding(0) @group(2) var<uniform> matrix_uniform: mat4x4<f32>;


@vertex
fn vs_main(
    vert: VertexInput,
    in: InstanceInput,
    @builtin(vertex_index) vert_index: u32,
) -> VertexOutput {
    var output: VertexOutput;
    var uvs = array<vec2<f32>, 4>(in.uv0, in.uv1, in.uv2, in.uv3);
    let matrix = create_translation(in.pos)
        * create_scale(vec3<f32>(in.scale.xy, 1.));
    output.color = in.color;
    output.tex_coord = uvs[vert_index % 4u];
    output.position = matrix_uniform * matrix * vec4<f32>(vert.position, 1.0);
    return output;
}


@binding(0) @group(1) var tex_2d: texture_2d<f32>;
@binding(0) @group(0) var tex_sampler: sampler;


@fragment
fn fs_main(in: VertexOutput) -> @location(0) vec4<f32> {
    return textureSample(tex_2d, tex_sampler, in.tex_coord) * in.color;
}

fn create_scale(scale_2d: vec3<f32>) -> mat4x4<f32> {
    return mat4x4<f32>(
        vec4<f32>(scale_2d.x, 0., 0., 0.),
        vec4<f32>(0., scale_2d.y, 0., 0.),
        vec4<f32>(0., 0., scale_2d.z, 0.),
        vec4<f32>(0., 0., 0., 1.),
    );
}

fn create_translation(pos: vec3<f32>) -> mat4x4<f32> {
    return mat4x4<f32>(
        vec4<f32>(1., 0., 0., 0.),
        vec4<f32>(0., 1., 0., 0.),
        vec4<f32>(0., 0., 1., 0.),
        vec4<f32>(pos.x, pos.y, pos.z, 1.)
    );
}