using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class InhaleBaseGroup : RenderGroup
{
    public InhaleBaseGroup(int groupId, int enemyTypeId)
    {
        GroupId = groupId;
        EnemyTypeId = enemyTypeId;

        useAnimation = false;
        currentCapacity = InitialCapacity;
        Enemies = new NativeList<EnemyInstance>(currentCapacity, Allocator.Persistent);
        Matrices = new NativeArray<Matrix4x4>(currentCapacity, Allocator.Persistent);
    }

    public override void Update(float deltaTime)
    {
        var enemies = Enemies;
        int count = enemies.Length;
        var enemiesArray = enemies.AsArray();

        var handle = new JobHandle();
        handle = EnemyGroupUpdater.Inhale(handle, enemiesArray, count, deltaTime, Physics.gravity);
        handle = EnemyGroupUpdater.PhysicsCollisionJob(handle, enemiesArray, count, deltaTime, Physics.gravity);

        EnemyGroupUpdater.InhalePostProcess(handle, enemiesArray, count, this);
    }
}