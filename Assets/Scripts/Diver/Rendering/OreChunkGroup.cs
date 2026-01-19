using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

public class OreChunkGroup : RenderGroup
{
    public OreChunkGroup(int enemyTypeId) : base(enemyTypeId)
    {
        useAnimation = false;
        DataContainer.ScaleArcheTypeArray = new NativeArray<ScaleArcheType>(currentCapacity, Allocator.Persistent);
    }

    public override void Update(float deltaTime)
    {
        var handle = new JobHandle();
        handle = EnemyGroupUpdater.ScaleTo(handle, DataContainer.EnemyArcheTypeArray, DataContainer.ScaleArcheTypeArray, Count, deltaTime);
        handle = EnemyGroupUpdater.InhaleDamage(handle, DataContainer.EnemyArcheTypeArray, Count, deltaTime, 30);
        EnemyGroupUpdater.InhaleDamagePostProcess(handle, DataContainer.EnemyArcheTypeArray, Count, this);
    }

    public override void SetEntityData(int index)
    {
        base.SetEntityData(index);

        var entity = new ScaleArcheType
        {
            TargetScale = Random.Range(0.7f, 1.2f),
            ScaleSpeed = 2
        };
        DataContainer.SetEntityData(ref entity, index);
    }
}
