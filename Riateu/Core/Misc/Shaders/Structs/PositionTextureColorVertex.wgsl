#define_import_path Structs::PositionTextureColorVertex

struct PositionTextureColorVertex {
    @location(0) position: vec3<f32>,
    @location(1) tex_coord: vec2<f32>,
    @location(2) color: vec4<f32>,
};