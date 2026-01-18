using System;

public class OreSpawner : EnemySpawnerBase
{
    protected override Func<RenderGroup> GetGroupFactory()
    {
        return () => new InhaleBaseGroup(enemyTypeId);
    }
}
