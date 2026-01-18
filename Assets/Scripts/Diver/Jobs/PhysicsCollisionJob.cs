using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct PhysicsCollisionMovementJob : IJobParallelFor
{
    public NativeArray<EnemyArcheType> Enemies;
    [ReadOnly] public NativeArray<RaycastHit> Hits;
    [ReadOnly] public float DeltaTime;
    [ReadOnly] public float3 Gravity;

    private const float Restitution = 0.2f;
    private const float LinearDrag = 0.3f;
    private const float KineticFriction = 0.3f;
    private const float StaticFriction = 0.5f;
    private const float StopVelocityThreshold = 0.1f;

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
            }
            
            float normalAccel = math.dot(enemy.Acceleration, normal);
            if (normalAccel < 0)
            {
                enemy.Acceleration -= normal * normalAccel;
            }
            float normalForce = math.abs(normalAccel);
            
            float3 tangentVelocity = enemy.Velocity - math.dot(enemy.Velocity, normal) * normal;
            float tangentSpeed = math.length(tangentVelocity);
            
            float3 tangentAccel = enemy.Acceleration - math.dot(enemy.Acceleration, normal) * normal;
            float tangentForce = math.length(tangentAccel);
            
            float maxStaticFriction = StaticFriction * normalForce;
            
            if (tangentSpeed < StopVelocityThreshold && tangentForce < maxStaticFriction)
            {
                enemy.Velocity = normal * math.dot(enemy.Velocity, normal);
                enemy.Acceleration -= tangentAccel;
            }
            else if (tangentSpeed > 0.001f)
            {
                float3 frictionDir = -tangentVelocity / tangentSpeed;
                enemy.Acceleration += frictionDir * KineticFriction * normalForce;
            }
            
            enemy.Velocity += enemy.Acceleration * DeltaTime;
        }

        enemy.Velocity *= math.exp(-LinearDrag * DeltaTime);
        
        enemy.Acceleration = float3.zero;
        Enemies[index] = enemy;
    }
}