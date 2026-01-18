using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct InhaleMarkDeadEnemiesJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<EnemyArchyType> Enemies;
    [ReadOnly] public float3 InhaleOrigin;
    [ReadOnly] public float CaptureDistanceSq;
    [WriteOnly] public NativeArray<bool> IsDead;

    public void Execute(int index)
    {
        var enemy = Enemies[index];
        float distSq = math.distancesq(enemy.Position, InhaleOrigin);
        IsDead[index] = distSq < CaptureDistanceSq;
    }
}