using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct ScaleJob : IJobParallelFor
{
    public NativeArray<EnemyArcheType> Enemies;
    [ReadOnly] public NativeArray<ScaleArcheType> Scales;
    [ReadOnly] public float DeltaTime;

    public void Execute(int index)
    {
        var enemy = Enemies[index];
        var scaleArcheType = Scales[index];
        enemy.Scale = math.min(scaleArcheType.TargetScale, enemy.Scale + scaleArcheType.ScaleSpeed * DeltaTime);
        Enemies[index] = enemy;
    }
}