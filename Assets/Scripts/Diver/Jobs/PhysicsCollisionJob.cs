using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct PhysicsCollisionJob : IJobParallelFor
{
    public NativeArray<EnemyInstance> Enemies;
    public NativeArray<RaycastHit> Hits;
    [ReadOnly] public float DeltaTime;
    [ReadOnly] public float3 Gravity;

    public void Execute(int index)
    {
        var enemy = Enemies[index];
        var hit = Hits[index];

        enemy.Acceleration += Gravity;
        
        if (hit.colliderEntityId == 0)
        {
            enemy.Position += enemy.Velocity * DeltaTime;
            enemy.Velocity += enemy.Acceleration * DeltaTime;
        }
        else
        {
            enemy.Velocity = math.reflect(enemy.Velocity, hit.normal) * 0.2f;
        }
        enemy.Acceleration = 0;

        Enemies[index] = enemy;
    }
}