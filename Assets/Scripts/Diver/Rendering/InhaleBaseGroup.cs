using Unity.Jobs;
using UnityEngine;

public class InhaleBaseGroup : RenderGroup
{
    public InhaleBaseGroup(int enemyTypeId) : base(enemyTypeId)
    {
        useAnimation = false;
    }

    public override void Update(float deltaTime)
    {
        var handle = new JobHandle();
        handle = EnemyGroupUpdater.Inhale(handle, DataContainer.EnemyArcheTypeArray, Count, deltaTime, Physics.gravity);
        handle = EnemyGroupUpdater.PhysicsCollisionJob(handle, DataContainer.EnemyArcheTypeArray, Count, deltaTime, Physics.gravity);

        EnemyGroupUpdater.InhalePostProcess(handle, DataContainer.EnemyArcheTypeArray, Count, this);
    }
}