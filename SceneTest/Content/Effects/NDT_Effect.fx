float4x4 World;
float4x4 View;
float4x4 Projection;

float3 DiffuseColor;
float Alpha = 1.0;
bool AlphaEnabled = true;
bool TextureEnabled;

bool LightingEnabled = true;

texture BasicTexture;
sampler BasicTextureSampler = sampler_state {
	texture = <BasicTexture>;

};
float NearPlane = 1;
float FarPlane = 400;

bool NormalMapEnabled = false;
texture NormalMap;
sampler NormalMapSampler = sampler_state {
	texture = <NormalMap>;

};

bool AlphaMaskEnabled = false;
texture AlphaMask;
sampler AlphaMaskSampler = sampler_state
{
    texture = <AlphaMask>;

};

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
    float3 Tangent : NORMAL1;
	float2 UV : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float3 Normal : TEXCOORD1;
	float4 WorldPosition : TEXCOORD2;
	float2 Depth : TEXCOORD3;
	float2 UV : TEXCOORD4;
    float3 Tangent : TEXCOORD5;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;
	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	output.WorldPosition = worldPosition;
    output.Normal = mul(input.Normal, World);
    output.Tangent = mul(input.Tangent, World);
	output.Depth = output.Position.zw;
	output.UV = input.UV;
	return output;
}

struct PixelShaderOutput
{ 
	float4 Normal : COLOR0;  
	float4 Depth : COLOR1; 
	float4 Texture : COLOR2;
    float4 Tangent : COLOR3;
};

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
	PixelShaderOutput output;
    
    float3 tangent = input.Tangent;
    tangent = normalize(tangent);
    
    float3 normal = input.Normal;
    normal = normalize(normal);
    
    float3 binormal = normalize(cross(tangent, normal));
    
    float3x3 TBN = float3x3(tangent, normal, binormal);
    
    normal = float3(0, 1, 0);
    
	if (NormalMapEnabled)
	{
		float3 normalMap = tex2D(NormalMapSampler, input.UV).xyz;
		normalMap = normalMap * 2 - 1;
        normal = normalize(normalMap).xzy;
    }

    output.Texture = float4(0, 0, 0, 0);
    
    normal = mul(normal, TBN).xyz;
    
    tangent /= 2;
    tangent += 0.5;    
    normal /= 2;
    normal += 0.5;
    output.Tangent = float4(tangent, 1);	
    output.Normal = float4(normal, 1);
	
    //non-linear depth buffer
    //float depth = ((1 / input.Depth.y) - (1 / NearPlane)) / ((1 / FarPlane) - (1 / NearPlane));
    
    //linear depth buffer
    //float depth = ((input.Depth.x) - (NearPlane)) / ((FarPlane) - (NearPlane));
    
    //more fun options
    float depth = input.Depth.x / input.Depth.y;
    
    output.Depth = float4(depth, depth, depth, 1);

	output.Texture = float4(1, 1, 1, Alpha);
	if (TextureEnabled == true)
	{
		output.Texture = float4(tex2D(BasicTextureSampler, input.UV).xyz, tex2D(BasicTextureSampler, input.UV).a * Alpha);
		//output.Texture *= float4(DiffuseColor, 1);
	}
	else
	{
		if (AlphaEnabled)
		{
			output.Texture = float4(DiffuseColor, Alpha);
		}
		else
		{
			output.Texture = float4(DiffuseColor, 1);
		}
	}

	if (TextureEnabled == true)
	{
        if (output.Texture.a <= 0.001)
		{
			if (AlphaEnabled)
			{
				output.Texture = float4(0, 0, 0, 0);
				output.Normal = float4(0, 0, 0, 0);
				discard;
			}
		}
	}
	//if (DiffuseColor.x == DiffuseColor.y == DiffuseColor.z == 0)
	//	DiffuseColor = float3(1, 1, 1);

	if (Alpha == 0)
	{
		if (AlphaEnabled)
		{
			output.Texture = float4(0, 0, 0, 0);
			output.Normal = float4(0, 0, 0, 0);
			discard;
		}
	}
    
    if (AlphaMaskEnabled)
        if (tex2D(AlphaMaskSampler, input.UV).r < 0.5)
            discard;
	
    return output;
}

technique Technique1
{
	pass Pass1
	{
		// TODO: set renderstates here.

		VertexShader = compile vs_4_0_level_9_3 VertexShaderFunction();
		PixelShader = compile ps_4_0_level_9_3 PixelShaderFunction();
	}
}
