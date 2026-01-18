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
        var enemies = Enemies;
        int count = enemies.Length;
        var enemiesArray = enemies.AsArray();

        var handle = new JobHandle();
        handle = EnemyGroupUpdater.Inhale(handle, enemiesArray, count, deltaTime, Physics.gravity);
        handle = EnemyGroupUpdater.PhysicsCollisionJob(handle, enemiesArray, count, deltaTime, Physics.gravity);

        EnemyGroupUpdater.InhalePostProcess(handle, enemiesArray, count, this);
    }
}