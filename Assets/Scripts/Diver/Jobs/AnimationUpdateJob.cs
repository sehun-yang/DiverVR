using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

[BurstCompile]
public struct AnimationUpdateJob : IJobParallelFor
{
    public NativeArray<EnemyArcheType> Enemies;
    [ReadOnly] public float DeltaTime;

    public void Execute(int index)
    {
        var enemy = Enemies[index];
        
        enemy.AnimationTime += DeltaTime;
        if (enemy.AnimationTime >= enemy.AnimationLength)
        {
            enemy.AnimationTime -= enemy.AnimationLength;
        }
        
        Enemies[index] = enemy;
    }
}
