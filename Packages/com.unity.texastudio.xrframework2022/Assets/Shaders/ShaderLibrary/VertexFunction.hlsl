//Created by Wans Wu
void GetPivotPainterData_float(float2 uv3,float2 uv4,float4 vertexColor,out float customAlpha,out float RandomPerElement,out float3 Xvector,out float3 PivotPosit)
{
    float3 vertColor = vertexColor.xyz * 2 - 1 ;
    //#if defined(ZAxis)
    //Xvextor = float3(-vertColor.x,-vertColor.y,vertColor.z);
    //#else
    Xvector = float3(-vertColor.x,vertColor.z,vertColor.y);
   // #endif
    customAlpha = vertexColor.w;
    //#if defined(ZAxis)
   // PivotPosit = float3(-uv3.x,uv3.y,uv4.x);
    //#else
    PivotPosit = float3(-uv3.x,uv4.x,-uv3.y);
    //#endif
    RandomPerElement = uv4.y;
    
}
float RandomRange( float2 Seed,float Min,float Max )
{
    float output =lerp(0,1,frac(sin(dot(Seed,float2(12.98,78.23))) * 43758.55 ));
    return output;
}
float inverseLerp(float A,float B,float alpha)
{
    return (alpha - A) / (B - A);
}

float3 RotateAroundAxis(  float3 Vertex, float3 PivotPos, float3 u, float Angle )
{
    Vertex -= PivotPos;
    float C = cos( Angle );
    float S = sin( Angle );
    float t = 1 - C;
    float m00 = t * u.x * u.x + C;
    float m01 = t * u.x * u.y - S * u.z;
    float m02 = t * u.x * u.z + S * u.y;
    float m10 = t * u.x * u.y + S * u.z;
    float m11 = t * u.y * u.y + C;
    float m12 = t * u.y * u.z - S * u.x;
    float m20 = t * u.x * u.z - S * u.y;
    float m21 = t * u.y * u.z + S * u.x;
    float m22 = t * u.z * u.z + C;
    float3x3 finalMatrix = float3x3( m00, m01, m02, m10, m11, m12, m20, m21, m22 );
    return mul( finalMatrix, Vertex ) + PivotPos;
}

float3 RotateAroundAxis_Flus (float3 Vertex,float3 PivotPos,float3 Axis,float Angle)
{
    float3 VertextToPivot = Vertex - PivotPos;
    float AngleRadians = radians(Angle) * 0.5;
    float3 sinAxis = normalize(Axis) * sin(AngleRadians);
    float3 R = cross(sinAxis,VertextToPivot) + VertextToPivot * cos(AngleRadians);
    return cross(sinAxis,R) * 2 + PivotPos + VertextToPivot;
}
float3 RotateXY (float3 R,float degrees)
{
    float3 reflUVW = R;
    half theta = degrees * PI / 180.0f;
    half costha = cos(theta);
    half sintha = sin(theta);
    reflUVW = half3(reflUVW.x * costha - reflUVW.z * sintha, reflUVW.y, reflUVW.x * sintha + reflUVW.z * costha);
    return reflUVW;
}


float3 GrassWind(float WindSpeed,float3 posWS,float windDir,float WindSizeBig,float WindSizeSmall,float windWeight,float windIntensity)
{
    //windDir
    float3 WindDir = float3(0,0,1);
    float3 RotateAxis = cross(WindDir,float3(0,1,0));
    //windSpeed
    float3 Speed = _Time.y * WindSpeed * float3(0.5,-0.5,-0.5);

    float3 windDirRotate = RotateXY(posWS,windDir);

    //BigTriangleWave
    float3 BigTriangleWave =  abs(frac(WindDir * Speed + (windDirRotate * rcp(WindSizeBig)) + 0.5) * 2 - 1) ;
    float FinalBigTriangleWave = dot(BigTriangleWave * BigTriangleWave * (3 - BigTriangleWave * 2 ),WindDir);

    //SmallTriangleWave
    float3 SmallTriangleWave = abs(frac(Speed + (windDirRotate * rcp(WindSizeSmall)) + 0.5) * 2 - 1);
    float FinalSmallTriangleWave = distance(SmallTriangleWave * SmallTriangleWave * (3 - SmallTriangleWave * 2),float3(0,0,0));

    float RatationAngle = FinalBigTriangleWave + FinalSmallTriangleWave * PI * 2;
    float3 pivotPoint = float3(0,0,0) - float3(0,0.1,0);
    
    return (RotateAroundAxis(posWS,pivotPoint,RotateAxis,RatationAngle) - posWS) * windWeight * windIntensity * 0.1;
    
    
}

