float4x4 ViewProjection;
float4x4 InverseViewProjection;
float4x4 InverseView;
float4x4 InverseProjection;
float3 Position;
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

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float posx = (2 * input.Position.x / w - 1);
	float posy = -(2 * input.Position.y / h - 1);
	float2 ScreenPosition = float2(posx, posy);
    
    float2 UV = input.Position.xy / float2(w, h);

    float3 Normal = (tex2D(NormalMapSampler, UV).xyz * 2) - 1;
    float Depth = (tex2D(DepthMapSampler, UV).x);
    
    float4 PreUnProject = float4(ScreenPosition.x, ScreenPosition.y, Depth, 1);
	float4 WorldPosition = mul(PreUnProject, InverseProjection);
    WorldPosition = WorldPosition / WorldPosition.w;
    WorldPosition = mul(WorldPosition, InverseView);
        
    return float4(WorldPosition.xyz, 1.0f);
}

technique Specular
{
	pass Pass0
	{
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 PixelShaderFunction();
	}
}