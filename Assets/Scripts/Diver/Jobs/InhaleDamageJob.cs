using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct InhaleDamageJob : IJobParallelFor
{
    public NativeArray<EnemyInstance> Enemies;
    [ReadOnly] public float MaxDamage;
    [ReadOnly] public float MaxInhaleRange;
    [ReadOnly] public float3 InhaleOrigin;
    [ReadOnly] public float3 ForwardDirection;
    [ReadOnly] public float ConeAngle;
    [ReadOnly] public float DeltaTime;

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
            
            if (cosAngle >= cosHalfAngle)
            {
                float damage = math.clamp(MaxDamage / len, 0, 10000);
                enemy.Health -= damage * DeltaTime;
                Enemies[index] = enemy;
            }
        }
    }
}