using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct AvoidJob : IJobParallelFor
{
    private const float RunSpeed = 5;

    public NativeArray<EnemyInstance> Enemies;
    [ReadOnly] public float3 MyPosition;
    [ReadOnly] public float DeltaTime;

    public void Execute(int index)
    {
        var enemy = Enemies[index];
        
        float3 myNextPos = enemy.Position + enemy.Velocity * DeltaTime;
        float3 away = myNextPos - MyPosition;
        if (math.lengthsq(away) < 5)
        {
            enemy.Velocity = math.normalize(away) * RunSpeed;
            Enemies[index] = enemy;
        }
    }
}
