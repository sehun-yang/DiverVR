using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

[BurstCompile]
public struct HealthDeadEnemiesJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<EnemyInstance> Enemies;
    [WriteOnly] public NativeArray<bool> IsDead;

    public void Execute(int index)
    {
        var enemy = Enemies[index];
        IsDead[index] = enemy.Health < 0;
    }
}