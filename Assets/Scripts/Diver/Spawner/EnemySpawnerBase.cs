using UnityEngine;

public abstract class EnemySpawnerBase : MonoBehaviour
{
    [Header("Spawn Settings")]
    public int enemyTypeId;
    public int maxSpawnAmount = 10;
    public float spawnBound = 5f;
    public bool autoSpawn = true;

    protected int groupId = -1;
    protected int currentSpawnCount;

    private void Update()
    {
        if (autoSpawn)
        {
            SpawnOne();
        }
    }

    public void SpawnAll()
    {
        int toSpawn = maxSpawnAmount - currentSpawnCount;
        for (int i = 0; i < toSpawn; i++)
        {
            SpawnOne();
        }
    }

    public void SpawnOne()
    {
        if (groupId < 0 || EnemyManager.Instance == null) return;
        if (currentSpawnCount >= maxSpawnAmount) return;

        Vector3 randomOffset = Random.insideUnitSphere * spawnBound;
        Vector3 position = transform.position + randomOffset;

        EnemyManager.Instance.SpawnEnemy(groupId, position);
        currentSpawnCount++;
    }

    public void DespawnOne()
    {
        if (groupId < 0 || EnemyManager.Instance == null) return;
        
        var group = EnemyManager.Instance.GetRenderGroup(groupId);
        if (group == null || group.Count == 0) return;

        EnemyManager.Instance.RemoveEnemy(groupId, group.Count - 1);
        currentSpawnCount--;
    }

    public void DespawnAll()
    {
        if (groupId < 0 || EnemyManager.Instance == null) return;

        var group = EnemyManager.Instance.GetRenderGroup(groupId);
        if (group == null) return;

        while (group.Count > 0)
        {
            EnemyManager.Instance.RemoveEnemy(groupId, group.Count - 1);
        }
        currentSpawnCount = 0;
    }

    private void OnDestroy()
    {
        if (groupId >= 0 && EnemyManager.Instance != null)
        {
            EnemyManager.Instance.RemoveRenderGroup(groupId);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnBound);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spawnBound * 2f);
    }
}
