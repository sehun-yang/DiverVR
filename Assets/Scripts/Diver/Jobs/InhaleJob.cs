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
    [ReadOnly] public float DeltaTime;

    public void Execute(int index)
    {
        var enemy = Enemies[index];
        
        float3 to = InhaleOrigin - enemy.Position;
        float len = math.length(to);
        if (len < MaxInhaleRange && len > 0)
        {
            enemy.Position += math.min(DeltaTime * InhaleStrength / len / len, len) * to;
            Enemies[index] = enemy;
        }
    }
}
