float Brightness;
int Width;
int Height;

texture DensityMap;
sampler DensityMapSampler = sampler_state
{
    texture = <DensityMap>;
    AddressU = clamp;
    AddressV = clamp;
    magfilter = LINEAR;
};

texture VelocityMap;
sampler VelocityMapSampler = sampler_state
{
    texture = <VelocityMap>;
    AddressU = clamp;
    AddressV = clamp;
    magfilter = LINEAR;
};

float3 BackgroundColor;

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

struct PixelShaderOutput
{
    float4 Density : COLOR0;
    float4 Velocity : COLOR1;
};

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
    PixelShaderOutput output;
    
    float posx = (2 * input.Position.x / Width - 1);
    float posy = -(2 * input.Position.y / Height - 1);
    float2 ScreenPosition = float2(posx, posy);
    
    float texx = (input.Position.x / Width);
    float texy = (input.Position.y / Height);
    float2 TexturePosition = float2(texx, texy);
    
    //velocity at point in qustion
    float2 velocity = tex2Dlod(VelocityMapSampler, float4(TexturePosition, 0.0f, 0.0f)).xy;
    
    //sample the density from the velocity point worked backwards
    float4 sample_position = float4(TexturePosition - velocity / (56000), 0.0f, 0.0f);
    
    //density sampled at that position, to be moved to current location
    float density = tex2Dlod(DensityMapSampler, sample_position).x;
   
    //4 sample positions near the previously sampled point
    float4 diffuse_sample_positions[4];
    diffuse_sample_positions[0] = sample_position + float4(2.0 / Width, 0.0, 0.0, 0.0);
    diffuse_sample_positions[1] = sample_position + float4(-2.0 / Width, 0.0, 0.0, 0.0);
    diffuse_sample_positions[2] = sample_position + float4(0.0, 2.0 / Height, 0.0, 0.0);
    diffuse_sample_positions[3] = sample_position + float4(0.0, -2.0 / Height, 0.0, 0.0);
    
    //find average density at that location
    float density_average = 0.25 * (tex2Dlod(DensityMapSampler, diffuse_sample_positions[0]).x +
                                    tex2Dlod(DensityMapSampler, diffuse_sample_positions[1]).x +
                                    tex2Dlod(DensityMapSampler, diffuse_sample_positions[2]).x +
                                    tex2Dlod(DensityMapSampler, diffuse_sample_positions[3]).x);
    
    //find average velocity at that point
    float2 velocity_average = 0.25 * (tex2Dlod(VelocityMapSampler, diffuse_sample_positions[0]).xy +
                                      tex2Dlod(VelocityMapSampler, diffuse_sample_positions[1]).xy +
                                      tex2Dlod(VelocityMapSampler, diffuse_sample_positions[2]).xy +
                                      tex2Dlod(VelocityMapSampler, diffuse_sample_positions[3]).xy);
    float2 velocity_sampled = tex2Dlod(VelocityMapSampler, sample_position).xy;
    
    //how fast to equalize nearby densities
    float density_diffusion_rate = 0.1;
    //how fast to equalize nearby velocities
    float velocity_diffusion_rate = 0.01;
    
    //calculate new density from translated location's average density
    float new_density = density + density_diffusion_rate * (density_average - density);
    //calculate new density from translated location's average density
    float2 new_velocity = velocity_sampled + velocity_diffusion_rate * (velocity_average - velocity_sampled);

    output.Density = float4(new_density, new_density * new_density, sqrt(new_density)/2, 1.0);
    output.Velocity = float4(velocity, 0.0, 1.0);
    
    return output;
}
/*
float4 PS_ADVECT_MACCORMACK(GS_OUTPUT_FLUIDSIMin, float timestep) : SV_Target
{ 
    // Trace back along the initial characteristic - we'll use    
    // values near this semi-Lagrangian "particle" to clamp our    
    // final advected value.    
    float3 cellVelocity = velocity.Sample(samPointClamp, in.CENTERCELL).xyz;
    float3 npos = in.cellIndex - timestep * cellVelocity; 
    // Find the cell corner closest to the "particle" and compute the    
    // texture coordinate corresponding to that location. 
    npos = floor(npos + float3(0.5f, 0.5f, 0.5f)); 
    npos = cellIndex2TexCoord(npos);
    // Get the values of nodes that contribute to the interpolated value.    
    // Texel centers will be a half-texel away from the cell corner.    
    float3 ht = float3(0.5f / textureWidth, 0.5f / textureHeight, 0.5f / textureDepth); 
    float4 nodeValues[8]; 
    nodeValues[0] = phi_n.Sample(samPointClamp, npos + float3(-ht.x, -ht.y, -ht.z)); 
    nodeValues[1] = phi_n.Sample(samPointClamp, npos + float3(-ht.x, -ht.y, ht.z)); 
    nodeValues[2] = phi_n.Sample(samPointClamp, npos + float3(-ht.x, ht.y, -ht.z)); 
    nodeValues[3] = phi_n.Sample(samPointClamp, npos + float3(-ht.x, ht.y, ht.z)); 
    nodeValues[4] = phi_n.Sample(samPointClamp, npos + float3(ht.x, -ht.y, -ht.z));
    nodeValues[5] = phi_n.Sample(samPointClamp, npos + float3(ht.x, -ht.y, ht.z));
    nodeValues[6] = phi_n.Sample(samPointClamp, npos + float3(ht.x, ht.y, -ht.z));
    nodeValues[7] = phi_n.Sample(samPointClamp, npos + float3(ht.x, ht.y, ht.z)); 
    // Determine a valid range for the result.    
    float4 phiMin = min(min(min(min(min(min(min(nodeValues[0],  nodeValues [1]), nodeValues [2]), nodeValues [3]), nodeValues[4]), nodeValues [5]), nodeValues [6]), nodeValues [7]); 
    float4 phiMax = max(max(max(max(max(max(max(nodeValues[0],  nodeValues [1]), nodeValues [2]), nodeValues [3]), nodeValues[4]), nodeValues [5]), nodeValues [6]), nodeValues [7]); 
    // Perform final advection, combining values from intermediate    
    // advection steps.    
    float4 r = phi_n_1_hat.Sample(samLinear, nposTC) + 0.5 * (phi_n.Sample(samPointClamp, in.CENTERCELL) - phi_n_hat.Sample(samPointClamp, in.CENTERCELL)); 
    // Clamp result to the desired range. 
    r = max(min(r, phiMax), phiMin); 
    return r; 
} 
*/
technique Specular
{
    pass Pass0
    {
        VertexShader = compile vs_4_0 VertexShaderFunction();
        PixelShader = compile ps_4_0 PixelShaderFunction();
    }
}