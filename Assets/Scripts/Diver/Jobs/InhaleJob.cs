using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct InhaleJob : IJobParallelFor
{
    public NativeArray<EnemyInstance> Enemies;
    [ReadOnly] public float MaxInhaleRange;
    [ReadOnly] public float InhaleStrength;
    [ReadOnly] public float3 InhaleOrigin;
    [ReadOnly] public float3 ForwardDirection;
    [ReadOnly] public float ConeAngle;
    [ReadOnly] public float DeltaTime;

    public void Execute(int index)
    {
        var enemy = Enemies[index];
        
        float3 to = enemy.Position - InhaleOrigin;
        float len = math.length(to);
        
        if (len > 0 && len < MaxInhaleRange)
        {
            float3 directionToEnemy = to / len;
            
            float cosAngle = math.dot(ForwardDirection, directionToEnemy);
            float cosHalfAngle = math.cos(math.radians(ConeAngle * 0.5f));
            
            if (len < 0.5f || cosAngle >= cosHalfAngle)
            {
                float3 inhaleDirection = -directionToEnemy;
                float force = math.min(DeltaTime * InhaleStrength / len, len);
                
                enemy.Acceleration += force * inhaleDirection;
                Enemies[index] = enemy;
            }
        }
    }
}