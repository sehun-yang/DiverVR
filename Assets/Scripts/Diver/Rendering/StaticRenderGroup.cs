using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class StaticRenderGroup : RenderGroup
{
    public StaticRenderGroup(int groupId, int enemyTypeId)
    {
        GroupId = groupId;
        EnemyTypeId = enemyTypeId;

        useAnimation = false;
        currentCapacity = InitialCapacity;
        Enemies = new NativeList<EnemyInstance>(currentCapacity, Allocator.Persistent);
        Matrices = new NativeArray<Matrix4x4>(currentCapacity, Allocator.Persistent);
    }

    public override void UpdateGroup(float deltaTime)
    {
        var enemies = Enemies;
        int count = enemies.Length;
        var enemiesArray = enemies.AsArray();

        var handle = new JobHandle();
        handle = EnemyGroupUpdater.Inhale(handle, enemiesArray, count, deltaTime, Physics.gravity);
        handle = EnemyGroupUpdater.PhysicsCollisionJob(handle, enemiesArray, count, deltaTime, Physics.gravity);

        NativeArray<bool> isDead = default;
        if (ModuleManager.Instance.InhaleModule.Enabled)
        {
            isDead = new NativeArray<bool>(count, Allocator.TempJob);
            handle = EnemyGroupUpdater.MarkDeadEnemies(handle, enemiesArray, count, isDead);
        }

        handle.Complete();

        if (ModuleManager.Instance.InhaleModule.Enabled)
        {
            EnemyGroupUpdater.RemoveDeadEnemies(this, isDead);
            isDead.Dispose();
        }
    }
}
