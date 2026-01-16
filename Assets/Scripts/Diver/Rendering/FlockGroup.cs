using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class FlockGroup : RenderGroup
{
    public float3 OriginPoint;
    public float MaxDistanceSq;
    public FlockSettings Settings;

    public FlockGroup(int groupId, int enemyTypeId, float3 origin, float maxDistance, FlockSettings settings)
    {
        GroupId = groupId;
        EnemyTypeId = enemyTypeId;
        OriginPoint = origin;
        MaxDistanceSq = maxDistance * maxDistance;
        Settings = settings;

        useAnimation = true;
        currentCapacity = InitialCapacity;
        Enemies = new NativeList<EnemyInstance>(currentCapacity, Allocator.Persistent);
        Matrices = new NativeArray<Matrix4x4>(currentCapacity, Allocator.Persistent);
        AnimationData = new NativeArray<float2>(currentCapacity, Allocator.Persistent);
    }

    public override void Update(float deltaTime)
    {
        var enemies = Enemies;
        int count = enemies.Length;
        var enemiesArray = enemies.AsArray();

        var handle = new JobHandle();
        handle = EnemyGroupUpdater.UpdateFlockGroup(handle, enemiesArray, count, this, deltaTime);
        handle = EnemyGroupUpdater.AvoidPlayer(handle, enemiesArray, count, deltaTime);
        handle = EnemyGroupUpdater.Inhale(handle, enemiesArray, count, deltaTime, Vector3.zero);
        handle = EnemyGroupUpdater.PhysicsCollisionJob(handle, enemiesArray, count, deltaTime, Vector3.zero);
        handle = EnemyGroupUpdater.UpdateAnimation(handle, enemiesArray, count, deltaTime);

        EnemyGroupUpdater.InhalePostProcess(handle, enemiesArray, count, this);
    }
}
