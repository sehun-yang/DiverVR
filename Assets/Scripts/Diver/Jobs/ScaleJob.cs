using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct ScaleJob : IJobParallelFor
{
    public NativeArray<EnemyInstance> Enemies;
    [ReadOnly] public float TargetScale;
    [ReadOnly] public float ScaleSpeed;
    [ReadOnly] public float DeltaTime;

    public void Execute(int index)
    {
        var enemy = Enemies[index];
        enemy.Scale = math.min(TargetScale, enemy.Scale + ScaleSpeed * DeltaTime);
        Enemies[index] = enemy;
    }
}