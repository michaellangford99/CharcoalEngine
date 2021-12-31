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

float SpecularPower = 200;
float SpecularIntensity = 1;
float LightAttenuation = 1000;
float3 LightPosition = float3(0, 1, 0);
float3 LightColor = float3(1, 1, 1);

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
    float4 WorldPosition4 = mul(PreUnProject, InverseProjection);
    WorldPosition4 = WorldPosition4 / WorldPosition4.w;
    float3 WorldPosition = mul(WorldPosition4, InverseView).xyz;
	
	// Perform the lighting calculations for a point light 
    float3 lightDirection = -normalize(WorldPosition - LightPosition);
	float lighting = clamp(dot(Normal, lightDirection)*2.0f, 0, 1);     
	// Attenuate the light to simulate a point light  
    float d = distance(LightPosition, WorldPosition);
	float att = 1 - d / LightAttenuation;// , 2);
	if (att < 0) att = 0;
	if (lighting < 0) lighting = 0;

	//specular highlights
	/*float3 light = -normalize(LightPosition-pos);
	normal = normalize(normal);
	float3 r = normalize(2 * dot(light, normal) * normal - light);
	float3 v = normalize(mul(-normalize(cam_pos-pos), world));

	float dotProduct = abs(dot(r, v));
	float specular = SpecularIntensity * max(pow(dotProduct, SpecularPower), 0);
	*/

	float3 light = -normalize(LightPosition - WorldPosition);
	Normal = normalize(Normal);
	float3 r = normalize(2 * dot(light, Normal) * Normal - light);
    float3 v = normalize(-normalize(CameraPosition - WorldPosition));

	float dotProduct = dot(r, v);
	float3 specular = SpecularIntensity * LightColor * max(pow(dotProduct, SpecularPower), 0);
			
	if (Depth == 0)
		return float4(0, 0, 0, 0);

	float4 output = float4(LightColor * (lighting * att) + specular, 1.0);

    output = output * tex2D(DiffuseMapSampler, UV);
	
	return output;

}

technique Technique1
{ 
	pass Pass1
	{ 
        VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 PixelShaderFunction(); 
	} 
}
