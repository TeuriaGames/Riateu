#define_import_path Structs::InstancedVertex

struct InstancedVertex {
    @location(1) pos: vec3<f32>,

    @location(2) uv0: vec2<f32>,
    @location(3) uv1: vec2<f32>,
    @location(4) uv2: vec2<f32>,
    @location(5) uv3: vec2<f32>,

    @location(6) scale: vec2<f32>,
    @location(7) origin: vec2<f32>,
    @location(8) rotation: f32,
    @location(9) color: vec4<f32>
};