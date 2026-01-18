using Unity.Jobs;

public class OreChunkGroup : RenderGroup
{
    public OreChunkGroup(int enemyTypeId) : base(enemyTypeId)
    {
        useAnimation = false;
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
