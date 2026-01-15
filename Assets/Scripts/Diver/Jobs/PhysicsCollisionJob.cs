using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct PhysicsCollisionMovementJob : IJobParallelFor
{
    public NativeArray<EnemyInstance> Enemies;
    [ReadOnly] public NativeArray<RaycastHit> Hits;
    [ReadOnly] public float DeltaTime;
    [ReadOnly] public float3 Gravity;

    private const float Restitution = 0.2f;
    private const float LinearDrag = 0.3f;
    private const float Friction = 0.3f;

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
            float3 normal = hit.normal;
            float normalVelocity = math.dot(enemy.Velocity, normal);
            
            if (normalVelocity < 0)
            {
                enemy.Velocity -= (1 + Restitution) * normalVelocity * normal;

                if (normalVelocity > -0.001f)
                {
                    enemy.Acceleration -= Friction * math.length(Gravity) * math.normalize(enemy.Velocity);
                }
            }
            
            float normalAccel = math.dot(enemy.Acceleration, normal);
            if (normalAccel < 0)
            {
                enemy.Acceleration -= normal * normalAccel;
            }
            
            enemy.Velocity += enemy.Acceleration * DeltaTime;
        }

        enemy.Velocity *= math.exp(-LinearDrag * DeltaTime);
        
        enemy.Acceleration = float3.zero;
        Enemies[index] = enemy;
    }
}