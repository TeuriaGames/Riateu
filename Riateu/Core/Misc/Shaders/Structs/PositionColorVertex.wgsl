#define_import_path Structs::PositionVertex

struct PositionColorVertex {
    @location(0) position: vec3<f32>,
    @location(1) color: vec4<f32>,
};