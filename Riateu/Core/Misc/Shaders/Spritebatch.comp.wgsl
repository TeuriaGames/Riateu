struct ComputeData {
    position: vec2<f32>,
    scale: vec2<f32>,
    origin: vec2<f32>,
    uv0: vec2<f32>,
    uv1: vec2<f32>,
    uv2: vec2<f32>,
    uv3: vec2<f32>,
    dimension: vec2<f32>,
    rotation: f32,
    color: vec4<f32>
};

struct PositionTextureColorVertex {
    position: vec4<f32>,
    tex_coords: vec2<f32>,
    color: vec4<f32>
};

@group(0)
@binding(0)
var<storage, read> computeData: array<ComputeData>;

@group(1)
@binding(0)
var<storage, read_write> vertexData: array<PositionTextureColorVertex>;

@compute
@workgroup_size(64, 1, 1)
fn main(@builtin(global_invocation_id) gID: vec3<u32>) {
    let n = gID.x;
    let compData = computeData[n];

    let origin = mat4x4<f32>(
        vec4<f32>(1., 0., 0., 0.),
        vec4<f32>(0., 1., 0., 0.),
        vec4<f32>(0., 0., 1., 0.),
        vec4<f32>(-compData.origin.x, -compData.origin.y, 1., 1.),
    );

    let val1 = cos(compData.rotation);
    let val2 = sin(compData.rotation);

    let x = compData.origin.x * (1. - val1) + compData.origin.y * val2;
    let y = compData.origin.y * (1. - val1) - compData.origin.x * val2;

    let rotation = mat4x4<f32>(
        val1, val2, 0., 0.,
        -val2, val1, 0., 0.,
        x, y, 1., 0.,
        0., 0., 0., 1.
    );

    let transform = origin * rotation;

    let width = compData.dimension.x * compData.scale.x;
    let height = compData.dimension.y * compData.scale.y;

    let topLeft = vec4<f32>(compData.position.x, compData.position.y, 1., 1.);
    let topRight= vec4<f32>(compData.position.x + width, compData.position.y, 1., 1.);
    let bottomLeft = vec4<f32>(compData.position.x, compData.position.y + height, 1., 1.);
    let bottomRight = vec4<f32>(compData.position.x + width, compData.position.y + height, 1., 1.);

    vertexData[n * 4u].position = vec4<f32>(
        (topLeft.x * transform[0][0]) + (topLeft.y * transform[1][0]) + transform[3][0],
        (topLeft.x * transform[0][1]) + (topLeft.y * transform[1][1]) + transform[3][1],
        (topLeft.x * transform[0][2]) + (topLeft.y * transform[1][2]) + transform[3][2],
        (topLeft.x * transform[0][3]) + (topLeft.y * transform[1][3]) + transform[3][3],
    );
    vertexData[n * 4u + 1u].position = vec4<f32>(
        (topRight.x * transform[0][0]) + (topRight.y * transform[1][0]) + transform[3][0],
        (topRight.x * transform[0][1]) + (topRight.y * transform[1][1]) + transform[3][1],
        (topRight.x * transform[0][2]) + (topRight.y * transform[1][2]) + transform[3][2],
        (topRight.x * transform[0][3]) + (topRight.y * transform[1][3]) + transform[3][3],
    );
    vertexData[n * 4u + 2u].position = vec4<f32>(
        (bottomLeft.x * transform[0][0]) + (bottomLeft.y * transform[1][0]) + transform[3][0],
        (bottomLeft.x * transform[0][1]) + (bottomLeft.y * transform[1][1]) + transform[3][1],
        (bottomLeft.x * transform[0][2]) + (bottomLeft.y * transform[1][2]) + transform[3][2],
        (bottomLeft.x * transform[0][3]) + (bottomLeft.y * transform[1][3]) + transform[3][3],
    );
    vertexData[n * 4u + 3u].position = vec4<f32>(
        (bottomRight.x * transform[0][0]) + (bottomRight.y * transform[1][0]) + transform[3][0],
        (bottomRight.x * transform[0][1]) + (bottomRight.y * transform[1][1]) + transform[3][1],
        (bottomRight.x * transform[0][2]) + (bottomRight.y * transform[1][2]) + transform[3][2],
        (bottomRight.x * transform[0][3]) + (bottomRight.y * transform[1][3]) + transform[3][3],
    );

    vertexData[n * 4u].tex_coords      = compData.uv0;
    vertexData[n * 4u + 1u].tex_coords = compData.uv1; 
    vertexData[n * 4u + 2u].tex_coords = compData.uv2;
    vertexData[n * 4u + 3u].tex_coords = compData.uv3;

    vertexData[n * 4u].color = compData.color;
    vertexData[n * 4u + 1u].color = compData.color;
    vertexData[n * 4u + 2u].color = compData.color;
    vertexData[n * 4u + 3u].color = compData.color;
}