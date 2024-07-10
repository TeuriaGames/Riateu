struct VertexInput {
    @location(0) color: vec4<f32>,
    @location(1) texCoord: vec2<f32>
};

@group(2)
@binding(0)
var texture: texture_2d<f32>;

@group(2)
@binding(1)
var texSampler: sampler;

@fragment
fn main(in: VertexInput) -> @location(0) vec4<f32> {
    let col = textureSample(texture, texSampler, in.texCoord) * in.color;
    if col.a == 0. {
        discard;
    }
    return col; 
}