using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class FlockGroup : RenderGroup
{
    public float3 OriginPoint;
    public float MaxDistanceSq;
    public FlockSettings Settings;

    public FlockGroup(int enemyTypeId, float3 origin, float maxDistance, FlockSettings settings)  : base(enemyTypeId)
    {
        OriginPoint = origin;
        MaxDistanceSq = maxDistance * maxDistance;
        Settings = settings;

        useAnimation = true;
        AnimationData = new NativeArray<float2>(currentCapacity, Allocator.Persistent);
    }

    public override void Update(float deltaTime)
    {
        var handle = new JobHandle();
        handle = EnemyGroupUpdater.UpdateFlockGroup(handle, DataContainer.EnemyArcheTypeArray, Count, this, deltaTime);
        handle = EnemyGroupUpdater.AvoidPlayer(handle, DataContainer.EnemyArcheTypeArray, Count, deltaTime);
        handle = EnemyGroupUpdater.Inhale(handle, DataContainer.EnemyArcheTypeArray, Count, deltaTime, Vector3.zero);
        handle = EnemyGroupUpdater.PhysicsCollisionJob(handle, DataContainer.EnemyArcheTypeArray, Count, deltaTime, Vector3.zero);
        handle = EnemyGroupUpdater.UpdateAnimation(handle, DataContainer.EnemyArcheTypeArray, Count, deltaTime);

        EnemyGroupUpdater.InhalePostProcess(handle, DataContainer.EnemyArcheTypeArray, Count, this);
    }
}
