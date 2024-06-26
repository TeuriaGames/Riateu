struct ComputeData {
    float2 position;
    float2 scale;
    float2 origin;
    float2 uv0_;
    float2 uv1_;
    float2 uv2_;
    float2 uv3_;
    float2 dimension;
    float rotation;
    int _pad9_0;
    int _pad9_1;
    int _pad9_2;
    float4 color;
};

struct PositionTextureColorVertex {
    float4 position;
    float2 tex_coords;
    int _pad2_0;
    int _pad2_1;
    float4 color;
};

ByteAddressBuffer computeData : register(t0);
RWByteAddressBuffer vertexData : register(u0, space1);

ComputeData ConstructComputeData(float2 arg0, float2 arg1, float2 arg2, float2 arg3, float2 arg4, float2 arg5, float2 arg6, float2 arg7, float arg8, float4 arg9) {
    ComputeData ret = (ComputeData)0;
    ret.position = arg0;
    ret.scale = arg1;
    ret.origin = arg2;
    ret.uv0_ = arg3;
    ret.uv1_ = arg4;
    ret.uv2_ = arg5;
    ret.uv3_ = arg6;
    ret.dimension = arg7;
    ret.rotation = arg8;
    ret.color = arg9;
    return ret;
}

[numthreads(64, 1, 1)]
void main(uint3 gID : SV_DispatchThreadID)
{
    uint n = gID.x;
    ComputeData compData = ConstructComputeData(asfloat(computeData.Load2(n*96+0)), asfloat(computeData.Load2(n*96+8)), asfloat(computeData.Load2(n*96+16)), asfloat(computeData.Load2(n*96+24)), asfloat(computeData.Load2(n*96+32)), asfloat(computeData.Load2(n*96+40)), asfloat(computeData.Load2(n*96+48)), asfloat(computeData.Load2(n*96+56)), asfloat(computeData.Load(n*96+64)), asfloat(computeData.Load4(n*96+80)));
    float4x4 origin = float4x4(float4(1.0, 0.0, 0.0, 0.0), float4(0.0, 1.0, 0.0, 0.0), float4(0.0, 0.0, 1.0, 0.0), float4(-(compData.origin.x), -(compData.origin.y), 1.0, 1.0));
    float4x4 translation = float4x4(float4(1.0, 0.0, 0.0, 0.0), float4(0.0, 1.0, 0.0, 0.0), float4(0.0, 0.0, 1.0, 0.0), float4(compData.position.x, compData.position.y, 1.0, 1.0));
    float val1_ = cos(compData.rotation);
    float val2_ = sin(compData.rotation);
    float x = ((compData.origin.x * (1.0 - val1_)) + (compData.origin.y * val2_));
    float y = ((compData.origin.y * (1.0 - val1_)) - (compData.origin.x * val2_));
    float4x4 rotation = float4x4(float4(val1_, val2_, 0.0, 0.0), float4(-(val2_), val1_, 0.0, 0.0), float4(x, y, 1.0, 0.0), float4(0.0, 0.0, 0.0, 1.0));
    float4x4 transform = mul(rotation, origin);
    float width = (compData.dimension.x * compData.scale.x);
    float height = (compData.dimension.y * compData.scale.y);
    float4 topLeft = float4(compData.position.x, compData.position.y, 1.0, 1.0);
    float4 topRight = float4((compData.position.x + width), compData.position.y, 1.0, 1.0);
    float4 bottomLeft = float4(compData.position.x, (compData.position.y + height), 1.0, 1.0);
    float4 bottomRight = float4((compData.position.x + width), (compData.position.y + height), 1.0, 1.0);
    vertexData.Store4(0+(n * 4u)*48, asuint(float4((((topLeft.x * transform[0].x) + (topLeft.y * transform[1].x)) + transform[3].x), (((topLeft.x * transform[0].y) + (topLeft.y * transform[1].y)) + transform[3].y), (((topLeft.x * transform[0].z) + (topLeft.y * transform[1].z)) + transform[3].z), (((topLeft.x * transform[0].w) + (topLeft.y * transform[1].w)) + transform[3].w))));
    vertexData.Store4(0+((n * 4u) + 1u)*48, asuint(float4((((topRight.x * transform[0].x) + (topRight.y * transform[1].x)) + transform[3].x), (((topRight.x * transform[0].y) + (topRight.y * transform[1].y)) + transform[3].y), (((topRight.x * transform[0].z) + (topRight.y * transform[1].z)) + transform[3].z), (((topRight.x * transform[0].w) + (topRight.y * transform[1].w)) + transform[3].w))));
    vertexData.Store4(0+((n * 4u) + 2u)*48, asuint(float4((((bottomLeft.x * transform[0].x) + (bottomLeft.y * transform[1].x)) + transform[3].x), (((bottomLeft.x * transform[0].y) + (bottomLeft.y * transform[1].y)) + transform[3].y), (((bottomLeft.x * transform[0].z) + (bottomLeft.y * transform[1].z)) + transform[3].z), (((bottomLeft.x * transform[0].w) + (bottomLeft.y * transform[1].w)) + transform[3].w))));
    vertexData.Store4(0+((n * 4u) + 3u)*48, asuint(float4((((bottomRight.x * transform[0].x) + (bottomRight.y * transform[1].x)) + transform[3].x), (((bottomRight.x * transform[0].y) + (bottomRight.y * transform[1].y)) + transform[3].y), (((bottomRight.x * transform[0].z) + (bottomRight.y * transform[1].z)) + transform[3].z), (((bottomRight.x * transform[0].w) + (bottomRight.y * transform[1].w)) + transform[3].w))));
    vertexData.Store2(16+(n * 4u)*48, asuint(compData.uv0_));
    vertexData.Store2(16+((n * 4u) + 1u)*48, asuint(compData.uv1_));
    vertexData.Store2(16+((n * 4u) + 2u)*48, asuint(compData.uv2_));
    vertexData.Store2(16+((n * 4u) + 3u)*48, asuint(compData.uv3_));
    vertexData.Store4(32+(n * 4u)*48, asuint(compData.color));
    vertexData.Store4(32+((n * 4u) + 1u)*48, asuint(compData.color));
    vertexData.Store4(32+((n * 4u) + 2u)*48, asuint(compData.color));
    vertexData.Store4(32+((n * 4u) + 3u)*48, asuint(compData.color));
    return;
}
