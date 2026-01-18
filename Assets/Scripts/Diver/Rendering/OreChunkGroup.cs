using Unity.Jobs;

public class OreChunkGroup : RenderGroup
{
    public OreChunkGroup(int enemyTypeId) : base(enemyTypeId)
    {
        useAnimation = false;
    }

    public override void Update(float deltaTime)
    {
        var handle = new JobHandle();
        handle = EnemyGroupUpdater.ScaleTo(handle, Enemies, Count, deltaTime, 1, 2);
        handle = EnemyGroupUpdater.InhaleDamage(handle, Enemies, Count, deltaTime, 30);
        EnemyGroupUpdater.InhaleDamagePostProcess(handle, Enemies, Count, this);
    }
}
