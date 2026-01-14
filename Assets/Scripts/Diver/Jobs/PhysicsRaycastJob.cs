using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct PhysicsRaycastJob : IJobParallelFor
{
    public NativeArray<EnemyInstance> Enemies;
    [WriteOnly] public NativeArray<SpherecastCommand> Commands;
    [ReadOnly] public float DeltaTime;

    private const int EnvironmentMask = 1 << 3;
    private const float RaycastPadding = 0.001f;

    public void Execute(int index)
    {
        var enemy = Enemies[index];
        Commands[index] = new SpherecastCommand(
            enemy.Position,
            enemy.BoundingRadius,
            math.normalizesafe(enemy.Velocity),
            new QueryParameters(EnvironmentMask, false, QueryTriggerInteraction.UseGlobal, false),
            math.length(enemy.Velocity) * DeltaTime + RaycastPadding
        );
    }
}