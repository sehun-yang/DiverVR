using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class OreChunkGroup : RenderGroup
{
    public OreChunkGroup(int enemyTypeId)
    {
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
        handle = EnemyGroupUpdater.ScaleTo(handle, enemiesArray, count, deltaTime, 1, 2);
        handle = EnemyGroupUpdater.InhaleDamage(handle, enemiesArray, count, deltaTime, 30);
        EnemyGroupUpdater.InhaleDamagePostProcess(handle, enemiesArray, count, this);
    }
}
