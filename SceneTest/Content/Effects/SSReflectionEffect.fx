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

float3 getDiffuse(float2 UV)
{
    return tex2D(DiffuseMapSampler, UV).xyz;
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

float3 WorldPositionToUVDepth(float3 pos, float4x4 vp)
{
    float4 proj_position = mul(float4(pos, 1.0), vp);
        
    proj_position /= proj_position.w;
        
    float proj_depth = proj_position.z;
                
    float2 UV = float2(proj_position.x + 1, -proj_position.y + 1) / 2;
    
    return float3(UV, proj_depth);

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
    
    if (length(Normal - float3(0, 1, 0)) > 0.8)
        return float4(getDiffuse(UV), 1);
    Normal = normalize(Normal + float3(0, 1, 0));
    //Normal = normalize(Normal + rand_f3(UV/100) / 30);
    float3 start_pos = WorldPosition;
    float3 reflect_dir = normalize(reflect(WorldPosition - CameraPosition, Normal));
    
    float3 test_pos = start_pos;
    float step = 0.04f;
    float MAX_STEPS = 500;
    
    [loop]for (int i = 0; i < MAX_STEPS; i++)
    {
        test_pos += reflect_dir * step;
        //if (i == 0)
        //    test_pos += reflect_dir * 3 * step;
        
        float3 uvdepth = WorldPositionToUVDepth(test_pos, ViewProjection);
        
        float2 test_UV = uvdepth.xy;
        float test_depth = uvdepth.z;
        float real_depth = getDepth(test_UV);
        
        //if (abs(real_depth - test_depth) < 0.02)
        if (real_depth < test_depth && (test_depth - real_depth < 0.2))
        {
            //if (abs(UV.x - test_UV.x) > (5 / w))
            //    if (abs(UV.y - test_UV.y) > (5 / h))
            return float4(getDiffuse(test_UV) * float3(120, 237, 280) / 255.0, 1.0);
        }
    }
    float l = length(getDiffuse(UV));
    return float4(getDiffuse(UV)*0.2, 1.0);
}

technique Technique1
{ 
	pass Pass1
	{ 
        VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 PixelShaderFunction(); 
	} 
}
