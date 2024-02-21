@binding(1) @group(1) var tex_sampler: sampler;
@binding(0) @group(1) var tex_2d: texture_2d<f32>;

@fragment
fn main(@location(0) color: vec4<f32>, @location(1) tex_coord: vec2<f32>) -> @location(0) vec4<f32> {
    return textureSample(tex_2d, tex_sampler, tex_coord) * color;
}