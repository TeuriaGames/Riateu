struct VertexInput {
    float4 position : LOC0;
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
    output.position = mul(in_.position, _expr8);
    VertexOutput _expr11 = output;
    const VertexOutput vertexoutput = _expr11;
    const VertexOutput_main vertexoutput_1 = { vertexoutput.color, vertexoutput.texCoord, vertexoutput.position };
    return vertexoutput_1;
}
