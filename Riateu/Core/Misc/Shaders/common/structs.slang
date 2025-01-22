struct Position2DTextureColor
{
    float2 Position : POSITION0;
    float2 TexCoord : TEXCOORD1;
    float4 Color : COLOR2;
};

struct PositionTextureColor
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD1;
    float4 Color : COLOR2;
};