using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

[BurstCompile]
public struct PhysicsMovementJob : IJobParallelFor
{
    public NativeArray<EnemyInstance> Enemies;
    [ReadOnly] public float DeltaTime;

    public void Execute(int index)
    {
        var enemy = Enemies[index];
        
        enemy.Velocity += enemy.Acceleration * DeltaTime;
        enemy.Position += enemy.Velocity * DeltaTime;
        enemy.Acceleration = 0;

        Enemies[index] = enemy;
    }
}