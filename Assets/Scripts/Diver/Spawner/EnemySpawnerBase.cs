using UnityEngine;

public abstract class EnemySpawnerBase : MonoBehaviour
{
    [Header("Spawn Settings")]
    public int enemyTypeId;
    public int maxSpawnAmount = 10;
    public float spawnBound = 5f;
    public bool autoSpawn = true;

    protected int groupId = -1;

    protected int CurrentSpawnCount => EnemyManager.Instance.GetCount(groupId);

    private void Update()
    {
        if (autoSpawn)
        {
            SpawnOne();
        }
    }

    public void SpawnAll()
    {
        int toSpawn = maxSpawnAmount - CurrentSpawnCount;
        SpawnN(toSpawn);
    }

    public void SpawnN(int count)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnOne();
        }
    }

    public void SpawnOne()
    {
        if (groupId < 0 || EnemyManager.Instance == null) return;
        if (autoSpawn && CurrentSpawnCount >= maxSpawnAmount) return;

        var (pos, rot) = GetSpawnPosition();

        EnemyManager.Instance.SpawnEnemy(groupId, pos, rot);
    }

    protected virtual (Vector3, Quaternion) GetSpawnPosition()
    {
        Vector3 randomOffset = Random.insideUnitSphere * spawnBound;
        return (transform.position + randomOffset, Quaternion.identity);
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
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spawnBound);
    }
}
