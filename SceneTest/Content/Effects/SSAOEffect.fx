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
    UV = max(UV, float2(0, 0));
    UV = min(UV, float2(1, 1));
    return (tex2D(DepthMapSampler, UV).x);
}

float rand_1_10(in float2 uv)
{
    float2 K1 = float2(
        23.14069263277926, // e^pi (Gelfond's constant)
         2.665144142690225 // 2^sqrt(2) (Gelfond–Schneider constant)
    );
    return frac(cos(dot(uv, K1)) * 12345.6789);
}

float2 rand_2_10(in float2 uv)
{
    float noiseX = rand_1_10(uv);
    float noiseY = rand_1_10(float2(uv.x, noiseX));
    return float2(noiseX, noiseY);
}

float3 rand_f3(in float2 uv)
{
    float2 r = rand_2_10(uv);
    float2 r2 = rand_2_10(r);
    return float3(r.y, rand_2_10(r2).y, r2.y);
}

float2 PositionToUV(float2 pos, float width, float height)
{
    float2 UV = pos / float2(width, height);
    return UV;
}

float2 ScreenPositionToUV(float2 sp, float width, float height)
{
    sp.x = (sp.x + 1) * width;
    sp.y = (-sp.y + 1) * height;
    return sp / float2(width, height);
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float posx = (2 * input.Position.x / w - 1);
    float posy = -(2 * input.Position.y / h - 1);
    float2 ScreenPosition = float2(posx, posy);
    
    float2 UV = PositionToUV(input.Position.xy, w, h);

    float3 Normal = getNormal(UV);
    float Depth = getDepth(UV);
    
	if (Depth == 0)
		return float4(0, 0, 0, 0);
    
    float4 PreUnProject = float4(ScreenPosition.x, ScreenPosition.y, Depth, 1);
    float4 WorldPosition4 = mul(PreUnProject, InverseProjection);
    WorldPosition4 = WorldPosition4 / WorldPosition4.w;
    float3 WorldPosition = mul(WorldPosition4, InverseView).xyz;
    #define PROBES 10
    float3 probe_vectors[PROBES];
    float2 seed = UV;
    float occlusion = 0.0;
    for (int i = 0; i < PROBES; i++)
    {
        float3 rand = rand_f3(seed);
        seed = rand.xy;
        //probe_vectors[i] = normalize(float3(-rand.x, abs(rand.y), -rand.z)) * (pow(i / (float)PROBES, 2));
        probe_vectors[i] = (rand*2 - 1) * (pow(i / (float) PROBES, 2));
        if (dot(Normal, probe_vectors[i]) < 0)
            probe_vectors[i] = -probe_vectors[i];        
        
        float radius = 0.1;
        probe_vectors[i] *= radius;
        
        //generate ortho basis
        //float3 tangent = normalize(float3(rand_f3(abs(probe_vectors[i].xy)) * 2 - 1)); //yeah just make it up
        //float3 binormal = normalize(cross(tangent, Normal));
        //tangent = normalize(cross(Normal, binormal)); //now orthogalize it
        //float3x3 TBN = float3x3(tangent, Normal, binormal);
        //probe_vectors[i] = mul(probe_vectors[i], TBN).xyz;
        
        float3 probe_position = WorldPosition + probe_vectors[i];
        
        float4 proj_probe_position = mul(float4(probe_position, 1.0), ViewProjection);
        
        proj_probe_position /= proj_probe_position.w;
        
        float proj_probe_depth = proj_probe_position.z;
                
        float2 probe_UV = float2(proj_probe_position.x + 1, -proj_probe_position.y + 1) / 2;
        
        if (probe_UV.x > 0 && probe_UV.x < 1)
        {
            if (probe_UV.y > 0 && probe_UV.y < 1)
            {
                float sceneDepth = getDepth(probe_UV);

                if (proj_probe_depth - sceneDepth < 0.1)
                    occlusion += max(proj_probe_depth - sceneDepth, 0);
            }
        }
    }
   
    return float4(1 - occlusion, 0, 0, 1.0);
}

technique Technique1
{ 
	pass Pass1
	{ 
        VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 PixelShaderFunction(); 
	} 
}
