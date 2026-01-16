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

        var (pos, rot, scale) = GetSpawnTRS();

        EnemyManager.Instance.SpawnEnemy(groupId, pos, rot, scale);
    }

    public void SpawnNAt(int count, Vector3 position)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnOneAt(position);
        }
    }

    public void SpawnOneAt(Vector3 position)
    {
        Vector3 randomOffset = Random.insideUnitSphere * 0.5f;
        EnemyManager.Instance.SpawnEnemy(groupId, position + randomOffset, Quaternion.identity, 1);
    }

    protected virtual (Vector3, Quaternion, float) GetSpawnTRS()
    {
        Vector3 randomOffset = Random.insideUnitSphere * spawnBound;
        return (transform.position + randomOffset, Quaternion.identity, 1);
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
