float4x4 ViewProjection;
float4x4 InverseViewProjection;
float4x4 InverseView;
float4x4 InverseProjection;
float w;
float h;
float3 CameraPosition;
float NearClip;
float FarClip;

texture NormalMap;
sampler NormalMapSampler = sampler_state
{
    texture = <NormalMap>;
};

texture DepthMap;
sampler DepthMapSampler = sampler_state
{
    texture = <DepthMap>;
};

texture DiffuseMap;
sampler DiffuseMapSampler = sampler_state
{
    texture = <DiffuseMap>;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    output.Position = input.Position;

    output.Position.z = 1.0;
	
    return output;
}

float3 getNormal(float2 UV)
{
    return (tex2D(NormalMapSampler, UV).xyz * 2) - 1;
}

float getDepth(float2 UV)
{
    return (tex2D(DepthMapSampler, UV).x);
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float posx = (2 * input.Position.x / w - 1);
    float posy = -(2 * input.Position.y / h - 1);
    float2 ScreenPosition = float2(posx, posy);
    
    float2 UV = input.Position.xy / float2(w, h);

    float3 Normal = getNormal(UV);
    float Depth = getDepth(UV);
    
	if (Depth == 0)
		return float4(0, 0, 0, 0);
    
    float4 PreUnProject = float4(ScreenPosition.x, ScreenPosition.y, Depth, 1);
    float4 WorldPosition4 = mul(PreUnProject, InverseProjection);
    WorldPosition4 = WorldPosition4 / WorldPosition4.w;
    float3 WorldPosition = mul(WorldPosition4, InverseView).xyz;
    
    float pix_width = 1.0 / w;
    float pix_height = 1.0 / h;
    
    float2 test_points[4] = { float2(pix_width, 0), float2(0, pix_height), float2(-pix_width, 0), float2(0, -pix_height) };
    float differential = 1.0;
    for (int i = 0; i < 4; i++)
    {
        differential -= 10000 * pow(getDepth(UV + test_points[i]) - Depth, 2);
    }
    return float4(differential, differential, differential, 1.0);

}

technique Technique1
{ 
	pass Pass1
	{ 
        VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 PixelShaderFunction(); 
	} 
}
