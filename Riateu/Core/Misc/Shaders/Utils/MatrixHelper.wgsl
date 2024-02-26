fn create_scale(scale_2d: vec3<f32>) -> mat4x4<f32> {
    return mat4x4<f32>(
        vec4<f32>(scale_2d.x, 0., 0., 0.),
        vec4<f32>(0., scale_2d.y, 0., 0.),
        vec4<f32>(0., 0., scale_2d.z, 0.),
        vec4<f32>(0., 0., 0., 1.),
    );
}

fn create_translation_2d(pos: vec2<f32>) -> mat4x4<f32> {
    return mat4x4<f32>(
        vec4<f32>(1., 0., 0., 0.),
        vec4<f32>(0., 1., 0., 0.),
        vec4<f32>(0., 0., 1., 0.),
        vec4<f32>(pos.x, pos.y, 0., 1.)
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