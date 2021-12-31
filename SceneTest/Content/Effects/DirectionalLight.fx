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
    
    float3 lightdir = normalize(float3(0, -1, 1));
    
    float light = saturate(dot(Normal, -lightdir));
    
    /*// Perform the lighting calculations for a point light 
	float3 lightDirection = normalize(LightPosition - pos);  
	float lighting = clamp(dot(normal, lightDirection)*2.0f, 0, 1);     
	// Attenuate the light to simulate a point light  
	float d = distance(LightPosition, pos);  
	float att = 1 - d / LightAttenuation;// , 2);
	if (att < 0) att = 0;
	if (lighting < 0) lighting = 0;*/
        
    float3 outcolor = tex2D(DiffuseMapSampler, UV).xyz * light;
    
    return float4(outcolor + WorldPosition.xyz/1000000, 1.0f);
}

technique Specular
{
	pass Pass0
	{
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 PixelShaderFunction();
	}
}