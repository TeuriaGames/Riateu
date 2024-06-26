// language: metal1.0
#include <metal_stdlib>
#include <simd/simd.h>

using metal::uint;

struct _mslBufferSizes {
    uint size0;
    uint size1;
};

struct type_1 {
    metal::float2 inner[4];
};
struct ComputeData {
    metal::float2 position;
    metal::float2 scale;
    metal::float2 origin;
    type_1 uv;
    metal::float2 dimension;
    float rotation;
    char _pad6[12];
    metal::float4 color;
};
struct PositionTextureColorVertex {
    metal::float4 position;
    metal::float2 tex_coords;
    char _pad2[8];
    metal::float4 color;
};
typedef ComputeData type_4[1];
typedef PositionTextureColorVertex type_5[1];

struct main_Input {
};
kernel void main_(
  metal::uint3 gID [[thread_position_in_grid]]
, device type_4 const& computeData [[user(fake0)]]
, device type_5& vertexData [[user(fake0)]]
, constant _mslBufferSizes& _buffer_sizes [[user(fake0)]]
) {
    uint n = gID.x;
    ComputeData compData = computeData[n];
    metal::float4x4 origin = metal::float4x4(metal::float4(1.0, 0.0, 0.0, 0.0), metal::float4(0.0, 1.0, 0.0, 0.0), metal::float4(0.0, 0.0, 1.0, 0.0), metal::float4(-(compData.origin.x), -(compData.origin.y), 1.0, 1.0));
    float val1_ = metal::cos(compData.rotation);
    float val2_ = metal::sin(compData.rotation);
    float x = (compData.origin.x * (1.0 - val1_)) + (compData.origin.y * val2_);
    float y = (compData.origin.y * (1.0 - val1_)) - (compData.origin.x * val2_);
    metal::float4x4 rotation = metal::float4x4(metal::float4(val1_, val2_, 0.0, 0.0), metal::float4(-(val2_), val1_, 0.0, 0.0), metal::float4(x, y, 1.0, 0.0), metal::float4(0.0, 0.0, 0.0, 1.0));
    metal::float4x4 transform = origin * rotation;
    float width = compData.dimension.x * compData.scale.x;
    float height = compData.dimension.y * compData.scale.y;
    metal::float4 topLeft = metal::float4(compData.position.x, compData.position.y, 1.0, 1.0);
    metal::float4 topRight = metal::float4(compData.position.x + width, compData.position.y, 1.0, 1.0);
    metal::float4 bottomLeft = metal::float4(compData.position.x, compData.position.y + height, 1.0, 1.0);
    metal::float4 bottomRight = metal::float4(compData.position.x + width, compData.position.y + height, 1.0, 1.0);
    vertexData[n * 4u].position = metal::float4(((topLeft.x * transform[0].x) + (topLeft.y * transform[1].x)) + transform[3].x, ((topLeft.x * transform[0].y) + (topLeft.y * transform[1].y)) + transform[3].y, ((topLeft.x * transform[0].z) + (topLeft.y * transform[1].z)) + transform[3].z, ((topLeft.x * transform[0].w) + (topLeft.y * transform[1].w)) + transform[3].w);
    vertexData[(n * 4u) + 1u].position = metal::float4(((topRight.x * transform[0].x) + (topRight.y * transform[1].x)) + transform[3].x, ((topRight.x * transform[0].y) + (topRight.y * transform[1].y)) + transform[3].y, ((topRight.x * transform[0].z) + (topRight.y * transform[1].z)) + transform[3].z, ((topRight.x * transform[0].w) + (topRight.y * transform[1].w)) + transform[3].w);
    vertexData[(n * 4u) + 2u].position = metal::float4(((bottomLeft.x * transform[0].x) + (bottomLeft.y * transform[1].x)) + transform[3].x, ((bottomLeft.x * transform[0].y) + (bottomLeft.y * transform[1].y)) + transform[3].y, ((bottomLeft.x * transform[0].z) + (bottomLeft.y * transform[1].z)) + transform[3].z, ((bottomLeft.x * transform[0].w) + (bottomLeft.y * transform[1].w)) + transform[3].w);
    vertexData[(n * 4u) + 3u].position = metal::float4(((bottomRight.x * transform[0].x) + (bottomRight.y * transform[1].x)) + transform[3].x, ((bottomRight.x * transform[0].y) + (bottomRight.y * transform[1].y)) + transform[3].y, ((bottomRight.x * transform[0].z) + (bottomRight.y * transform[1].z)) + transform[3].z, ((bottomRight.x * transform[0].w) + (bottomRight.y * transform[1].w)) + transform[3].w);
    vertexData[n * 4u].tex_coords = compData.uv.inner[0];
    vertexData[(n * 4u) + 1u].tex_coords = compData.uv.inner[1];
    vertexData[(n * 4u) + 2u].tex_coords = compData.uv.inner[2];
    vertexData[(n * 4u) + 3u].tex_coords = compData.uv.inner[3];
    vertexData[n * 4u].color = compData.color;
    vertexData[(n * 4u) + 1u].color = compData.color;
    vertexData[(n * 4u) + 2u].color = compData.color;
    vertexData[(n * 4u) + 3u].color = compData.color;
    return;
}
