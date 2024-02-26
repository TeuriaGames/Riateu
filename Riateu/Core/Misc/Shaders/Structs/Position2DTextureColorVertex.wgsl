#define_import_path Structs::Position2DTextureColorVertex

struct Position2DTextureColorVertex {
    @location(0) position: vec2<f32>,
    @location(1) tex_coord: vec2<f32>,
    @location(2) color: vec4<f32>,
};