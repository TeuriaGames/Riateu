struct VertexInput {
    float2 position : LOC0;
    float2 texCoord : LOC1;
    float4 color : LOC2;
};

struct VertexOutput {
    float4 position : SV_Position;
    float4 color : LOC0;
    float2 texCoord : LOC1;
};

cbuffer MatrixUniform : register(b0, space1) { row_major float4x4 MatrixUniform; }

struct VertexOutput_main {
    float4 color : LOC0;
    float2 texCoord : LOC1;
    float4 position : SV_Position;
};

VertexOutput_main main(VertexInput in_)
{
    VertexOutput output = (VertexOutput)0;

    output.color = in_.color;
    output.texCoord = in_.texCoord;
    float4x4 _expr8 = MatrixUniform;
    output.position = mul(float4(in_.position, 0.0, 1.0), _expr8);
    VertexOutput _expr14 = output;
    const VertexOutput vertexoutput = _expr14;
    const VertexOutput_main vertexoutput_1 = { vertexoutput.color, vertexoutput.texCoord, vertexoutput.position };
    return vertexoutput_1;
}
