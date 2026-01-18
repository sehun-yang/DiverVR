using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct InhaleJob : IJobParallelFor
{
    public NativeArray<EnemyArchyType> Enemies;
    [ReadOnly] public float MaxInhaleRange;
    [ReadOnly] public float InhaleStrength;
    [ReadOnly] public float3 InhaleOrigin;
    [ReadOnly] public float3 ForwardDirection;
    [ReadOnly] public float ConeAngle;
    [ReadOnly] public float3 Gravity;
    [ReadOnly] public float DeltaTime;

    private const float CaptureRadius = 0.8f;
    private const float SpiralInwardSpeed = 4;
    private const float SpiralAngularSpeed = 24;
    private const float RadiusShrinkSpeed = 3f;

    public void Execute(int index)
    {
        var enemy = Enemies[index];
        
        float3 toEnemy = enemy.Position - InhaleOrigin;
        float len = math.length(toEnemy);
        
        if (len > 0 && len < MaxInhaleRange)
        {
            float3 directionToEnemy = toEnemy / len;
            
            float cosAngle = math.dot(ForwardDirection, directionToEnemy);
            float cosHalfAngle = math.cos(math.radians(ConeAngle * 0.5f));
            
            if (len < CaptureRadius || cosAngle >= cosHalfAngle)
            {
                enemy.Rotation = math.mul(enemy.Rotation, quaternion.EulerXYZ(10 * DeltaTime / len, 20 * DeltaTime / len, 30 * DeltaTime / len));
                if (len < CaptureRadius)
                {
                    enemy.Velocity = float3.zero;
                    
                    float3 axis = ForwardDirection;
                    
                    float alongAxis = math.dot(toEnemy, axis);
                    float3 radialOffset = toEnemy - alongAxis * axis;
                    float radius = math.length(radialOffset);
                    
                    float3 axialPull = math.sign(alongAxis) * SpiralInwardSpeed * -axis;
                    
                    float3 radialPull = float3.zero;
                    if (radius > 0.01f)
                    {
                        float3 radialDir = radialOffset / radius;
                        radialPull = -radialDir * RadiusShrinkSpeed;
                    }
                    
                    float3 spiral = float3.zero;
                    if (radius > 0.01f)
                    {
                        float3 radialDir = radialOffset / radius;
                        float3 tangent = math.cross(axis, radialDir);
                        spiral = tangent * SpiralAngularSpeed * radius;
                    }
                    
                    enemy.Position += (axialPull + radialPull + spiral) * DeltaTime;
                }
                else
                {
                    float t = 1f - (len / MaxInhaleRange);
                    
                    float3 desiredVelocity = -directionToEnemy * InhaleStrength;
                    float3 steering = desiredVelocity - enemy.Velocity;
                    
                    float steerStrength = math.lerp(InhaleStrength * 0.5f, InhaleStrength * 2f, t);
                    steering = ClampMagnitude(steering, steerStrength);
                    
                    enemy.Acceleration -= Gravity * t;
                    enemy.Acceleration += steering;
                }
                
                Enemies[index] = enemy;
            }
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float3 ClampMagnitude(float3 v, float max)
    {
        float len = math.length(v);
        if (len > max && len > 0.01f) return v * (max / len);
        return v;
    }
}