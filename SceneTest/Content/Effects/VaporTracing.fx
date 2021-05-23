float4x4 ViewProjection;
float4x4 InverseViewProjection;
float4x4 World;
float4x4 InverseWorld;
float3 CameraPosition;
float3 CornerMin;
float3 CornerMax;
float NearClip;
float FarClip;
float Brightness;
float w;
float h;

int GranularityX;
int GranularityY;
int GranularityZ;

texture DensityMap;
sampler DensityMapSampler = sampler_state {
texture = <DensityMap>;
    AddressU = clamp;
    AddressV = clamp;
    magfilter = POINT;
    minfilter = POINT;
    mipfilter = POINT;
};

float3 BackgroundColor;

float t;


float DistanceToPlane(float3 P, float K, float3 Ray, float3 Origin)
{
    return (K - dot(Origin, P)) / (dot(Ray, P));
}

float DistanceToBox(float3 Min, float3 Max, float3 Position, float3 Ray)
{
    //if position is already in the box, return 0
    if (Position.x < CornerMax.x && Position.x > CornerMin.x && Position.y < CornerMax.y && Position.y > CornerMin.y && Position.z < CornerMax.z && Position.z > CornerMin.z)
    {
        return 0;
    }
    
    float XPlaneMin = DistanceToPlane(float3(1, 0, 0), Min.x, Ray, Position);
    float XPlaneMax = DistanceToPlane(float3(1, 0, 0), Max.x, Ray, Position);
    float YPlaneMin = DistanceToPlane(float3(0, 1, 0), Min.y, Ray, Position);
    float YPlaneMax = DistanceToPlane(float3(0, 1, 0), Max.y, Ray, Position);
    float ZPlaneMin = DistanceToPlane(float3(0, 0, 1), Min.z, Ray, Position);
    float ZPlaneMax = DistanceToPlane(float3(0, 0, 1), Max.z, Ray, Position);
    
    if (XPlaneMin < 0.01)
        XPlaneMin = FarClip;
    if (XPlaneMax < 0.01)
        XPlaneMax = FarClip;
    
    if (YPlaneMin < 0.01)
        YPlaneMin = FarClip;
    if (YPlaneMax < 0.01)
        YPlaneMax = FarClip;
    
    if (ZPlaneMin < 0.01)
        ZPlaneMin = FarClip;
    if (ZPlaneMax < 0.01)
        ZPlaneMax = FarClip;
    
    float3 XPlaneMinPoint = Position + Ray * XPlaneMin;
    float3 XPlaneMaxPoint = Position + Ray * XPlaneMax;
    float3 YPlaneMinPoint = Position + Ray * YPlaneMin;
    float3 YPlaneMaxPoint = Position + Ray * YPlaneMax;
    float3 ZPlaneMinPoint = Position + Ray * ZPlaneMin;
    float3 ZPlaneMaxPoint = Position + Ray * ZPlaneMax;
    
    if (XPlaneMinPoint.y > CornerMax.y || XPlaneMinPoint.y < CornerMin.y || XPlaneMinPoint.z > CornerMax.z || XPlaneMinPoint.z < CornerMin.z)
    {
        XPlaneMin = FarClip;
    }
    if (XPlaneMaxPoint.y > CornerMax.y || XPlaneMaxPoint.y < CornerMin.y || XPlaneMaxPoint.z > CornerMax.z || XPlaneMaxPoint.z < CornerMin.z)
    {
        XPlaneMax = FarClip;
    }
    
    if (YPlaneMinPoint.x > CornerMax.x || YPlaneMinPoint.x < CornerMin.x || YPlaneMinPoint.z > CornerMax.z || YPlaneMinPoint.z < CornerMin.z)
    {
        YPlaneMin = FarClip;
    }
    if (YPlaneMaxPoint.x > CornerMax.x || YPlaneMaxPoint.x < CornerMin.x || YPlaneMaxPoint.z > CornerMax.z || YPlaneMaxPoint.z < CornerMin.z)
    {
        YPlaneMax = FarClip;
    }
    
    if (ZPlaneMinPoint.x > CornerMax.x || ZPlaneMinPoint.x < CornerMin.x || ZPlaneMinPoint.y > CornerMax.y || ZPlaneMinPoint.y < CornerMin.y)
    {
        ZPlaneMin = FarClip;
    }
    if (ZPlaneMaxPoint.x > CornerMax.x || ZPlaneMaxPoint.x < CornerMin.x || ZPlaneMaxPoint.y > CornerMax.y || ZPlaneMaxPoint.y < CornerMin.y)
    {
        ZPlaneMax = FarClip;
    }
    
    return min(min(ZPlaneMin, ZPlaneMax), min(min(XPlaneMin, XPlaneMax), min(YPlaneMin, YPlaneMax)));
}

int3 GetVoxelIndices(float3 CMin, float3 CMax, float3 Position, float VoxelsPerUnit)
{
    return int3(round(VoxelsPerUnit * (Position - CMin) / (CMax - CMin)));
}

/*float3 GetDensityAtVoxel(int3 Voxel, int Granularity)
{
    return tex2D(DensityMapSampler, Voxel.xy / (float) Granularity);
}

/*
int3 GetVoxelIndices(float3 CMin, float3 Position, float VoxelsPerUnit)
{
    float3 Pos = Position - CMin;
    
    Pos *= VoxelsPerUnit;
    
    return int3(round(Pos));
}*/
float3 GetColor()
{
    float x_mul = (1 + sin(t + (0 * 3.14) / 3));
    float y_mul = (1 + sin(t + (2 * 3.14) / 3));
    float z_mul = (1 + sin(t + (4 * 3.14) / 3));
    
    float3 multiplier = float3(x_mul, y_mul, z_mul) / 3;
    return float3(1, 1, 1);
    //return float3(0.70 + 0.3 * sin(t), 0.3 + 0.7 * sin(t), 1);
}

float2 GetUVofVoxel(int3 Voxel, int3 Granularity)
{
    return float2((float) Voxel.x / Granularity.x, (float) Voxel.y / (Granularity.y * Granularity.z) + (float) Voxel.z / Granularity.z);
}

float fade(float t)
{
    // Fade function as defined by Ken Perlin.  This eases coordinate values
    // so that they will ease towards integral values.  This ends up smoothing
    // the final output.
    return t * t * t * (t * (t * 6 - 15) + 10); // 6t^5 - 15t^4 + 10t^3
}

float GetDensityAtVoxel(float3 FVoxel, int3 Granularity)
{
    
    /*float3 position = ceil(FVoxel) - floor(FVoxel);
    
    int3 v_000 = (int3) floor(FVoxel);
    int3 v_001 = v_000 + int3(0, 0, 1);
    int3 v_010 = v_000 + int3(0, 1, 0);
    int3 v_011 = v_000 + int3(0, 1, 1);
    int3 v_100 = v_000 + int3(1, 0, 0);
    int3 v_101 = v_000 + int3(1, 0, 1);
    int3 v_110 = v_000 + int3(1, 1, 0);
    int3 v_111 = v_000 + int3(1, 1, 1);
    
    float i_000 = tex2Dlod(DensityMapSampler, float4(GetUVofVoxel(v_000, Granularity), 0.0f, 0.0f)).x;
    float i_001 = tex2Dlod(DensityMapSampler, float4(GetUVofVoxel(v_001, Granularity), 0.0f, 0.0f)).x;
    float i_010 = tex2Dlod(DensityMapSampler, float4(GetUVofVoxel(v_010, Granularity), 0.0f, 0.0f)).x;
    float i_011 = tex2Dlod(DensityMapSampler, float4(GetUVofVoxel(v_011, Granularity), 0.0f, 0.0f)).x;
    float i_100 = tex2Dlod(DensityMapSampler, float4(GetUVofVoxel(v_100, Granularity), 0.0f, 0.0f)).x;
    float i_101 = tex2Dlod(DensityMapSampler, float4(GetUVofVoxel(v_101, Granularity), 0.0f, 0.0f)).x;
    float i_110 = tex2Dlod(DensityMapSampler, float4(GetUVofVoxel(v_110, Granularity), 0.0f, 0.0f)).x;
    float i_111 = tex2Dlod(DensityMapSampler, float4(GetUVofVoxel(v_111, Granularity), 0.0f, 0.0f)).x;
    
    float LerpSquare1 = lerp(
                             lerp(i_000, i_100, fade(position.x)),
                             lerp(i_010, i_110, fade(position.x)),
                             fade(position.y));

    float LerpSquare2 = lerp(
                             lerp(i_001, i_101, fade(position.x)),
                             lerp(i_011, i_111, fade(position.x)),
                             fade(position.y));

    float result = lerp(LerpSquare1, LerpSquare2, fade(position.z));
    */
    
    float result = tex2Dlod(DensityMapSampler, float4(GetUVofVoxel(FVoxel, Granularity), 0.0f, 0.0f)).x;
    
    
    /*float x_mul = (1 + sin(t + (0 * 3.14) / 3));
    float y_mul = (1 + sin(t + (2 * 3.14) / 3));
    float z_mul = (1 + sin(t + (4 * 3.14) / 3));
    
    float3 multiplier = float3(1.0f, 0, 0);*/
    //float3(x_mul, y_mul, z_mul) / 3;
    
    return result * Brightness;
}

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

	float4 PreUnProject = float4(ScreenPosition.x, ScreenPosition.y, 0.0, 1);
	float4 WorldPosition = mul(PreUnProject, InverseViewProjection);

	float4 PreUnProjectCenter = float4(0, 0, 0, 1);
	float4 WorldPositionCenter = mul(PreUnProjectCenter, InverseViewProjection);

    WorldPosition = WorldPosition / WorldPosition.w;
	WorldPositionCenter = WorldPositionCenter / WorldPositionCenter.w;

	float3 Ray = normalize(WorldPosition.xyz - CameraPosition);
	float3 RayCenter = normalize(WorldPositionCenter.xyz - CameraPosition);
	
	float3 MarchPos = CameraPosition;
    
    float4 CMin = mul(float4(CornerMin, 1), World);
    CornerMin = (CMin / CMin.w).xyz;
    float4 CMax = mul(float4(CornerMax, 1), World);
    CornerMax = (CMax / CMax.w).xyz;
    
    float dist = DistanceToBox(CornerMin, CornerMax, CameraPosition, Ray);
    
    if (dist == FarClip)
    {
        return float4(BackgroundColor, 1);
    }
    
    float3 InterSectionPoint = CameraPosition + Ray * dist;
    
    float3 AlterStartPoint = CameraPosition + Ray * dist + Ray * distance(CornerMax, CornerMin);
    float alter_dist = DistanceToBox(CornerMin, CornerMax, AlterStartPoint, -Ray);
    float3 AlterInterSectionPoint = AlterStartPoint + (-Ray) * alter_dist;
    
    //accessing the texture
    //float3 internalposition = InterSectionPoint - CornerMin;
    
    //float granularity = (float) Granularity;
    
    float intensity = 0.0f;// = distance(AlterInterSectionPoint, InterSectionPoint) / 5;
    
    float3 CenteredIntersectionPoint = InterSectionPoint - CornerMin;
    float3 CenteredAlterIntersectionPoint = AlterInterSectionPoint - CornerMin;
    
    float3 ScaledCenteredIntersectionPoint = CenteredIntersectionPoint / (CornerMax - CornerMin);
    float3 ScaledCenteredAlterIntersectionPoint = CenteredAlterIntersectionPoint / (CornerMax - CornerMin);
    
    float3 Granularity = int3(GranularityX, GranularityY, GranularityZ);
    
    ScaledCenteredIntersectionPoint *= Granularity;
    ScaledCenteredAlterIntersectionPoint *= Granularity;
    /*
    float steps = distance(ScaledCenteredIntersectionPoint, ScaledCenteredAlterIntersectionPoint);
    
    float3 Voxel = ScaledCenteredIntersectionPoint;
    float3 VRay = -normalize(ScaledCenteredIntersectionPoint - ScaledCenteredAlterIntersectionPoint);
    
    for (int i = 0; i < trunc(steps); i++)
    {
        Voxel += VRay;
        intensity += GetDensityAtVoxel(Voxel, Granularity);
        
        if (i == trunc(steps) - 1)
        {
            //last step
            Voxel += VRay * (steps - trunc(steps));
            intensity += GetDensityAtVoxel(Voxel, Granularity);
        }
    }*/
    
    int steps = trunc(distance(ScaledCenteredIntersectionPoint, ScaledCenteredAlterIntersectionPoint));
    float partial_step = distance(ScaledCenteredIntersectionPoint, ScaledCenteredAlterIntersectionPoint) - steps;
    float distance_through_box = distance(ScaledCenteredIntersectionPoint, AlterInterSectionPoint);
    float step_length = distance_through_box / (float) steps;
    float3 position_in_box = ScaledCenteredIntersectionPoint;
    float3 VRay = -normalize(ScaledCenteredIntersectionPoint - ScaledCenteredAlterIntersectionPoint);
    for (int i = 0; i < steps; i++)
    {
        position_in_box += VRay * step_length;
        intensity += GetDensityAtVoxel(position_in_box, Granularity);
        
        if (i == steps - 1)
        {
            //last step
            intensity += GetDensityAtVoxel(position_in_box, Granularity) * partial_step;
        }

    }
    
    return float4(lerp(BackgroundColor, GetColor(), intensity), 1);
}

technique Specular
{
	pass Pass0
	{
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 PixelShaderFunction();
	}
}