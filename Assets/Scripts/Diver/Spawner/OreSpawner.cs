using UnityEngine;

public class OreSpawner : EnemySpawnerBase
{
    private void Start()
    {
        if (EnemyManager.Instance == null)
        {
            Debug.LogError("EnemyManager not found!");
            return;
        }

        var group = new InhaleBaseGroup(groupId, enemyTypeId);
        groupId = EnemyManager.Instance.RegisterRenderGroup(group);
    }
}
