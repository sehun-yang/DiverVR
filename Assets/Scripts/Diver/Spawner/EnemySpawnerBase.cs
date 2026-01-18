using System;
using UnityEngine;

public abstract class EnemySpawnerBase : MonoBehaviour
{
    [Header("Spawn Settings")]
    public int enemyTypeId;
    public int maxSpawnAmount = 10;
    public float spawnBound = 5f;

    protected uint spawnerId = 0;
    protected RenderGroup group;
    protected int CurrentSpawnCount = 0;

    protected virtual void Start()
    {
        if (EnemyManager.Instance == null)
        {
            Debug.LogError("EnemyManager not found!");
            return;
        }

        spawnerId = EnemyManager.Instance.IssueSpawnerId(this);
        group = EnemyManager.Instance.GetOrAddRenderGroup(GetGroupFactory(), enemyTypeId);
    }

    private void Update()
    {
        SpawnOne();
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
        if (group == null || EnemyManager.Instance == null) return;
        if (CurrentSpawnCount >= maxSpawnAmount) return;

        var (pos, rot, scale) = GetSpawnTRS();

        EnemyManager.Instance.SpawnEnemy(group, spawnerId, pos, rot, scale);
        CurrentSpawnCount += 1;
    }

    public static void SpawnNAt(int enemyTypeId, int count, Vector3 position, Quaternion rotation, float scale)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnOneAt(enemyTypeId, position, rotation, scale);
        }
    }

    public static void SpawnOneAt(int enemyTypeId, Vector3 position, Quaternion rotation, float scale)
    {
        var group = EnemyManager.Instance.GetOrAddRenderGroup(null, enemyTypeId);
        if (group != null)
        {
            Vector3 randomOffset = UnityEngine.Random.insideUnitSphere * 0.5f;
            EnemyManager.Instance.SpawnEnemy(group, 0, position + randomOffset, rotation, scale);
        }
        else
        {
            Debug.LogWarning("Render group must exist before static spawnings");
        }
    }

    public void OneRemoved(ref EnemyArcheType instance)
    {
        CurrentSpawnCount -= 1;
        OnOneRemoved(ref instance);
    }

    protected virtual (Vector3, Quaternion, float) GetSpawnTRS()
    {
        Vector3 randomOffset = UnityEngine.Random.insideUnitSphere * spawnBound;
        return (transform.position + randomOffset, Quaternion.identity, 1);
    }

    protected virtual void OnOneRemoved(ref EnemyArcheType instance)
    {
        var deathEffect = EnemyManager.Instance.enemyDataAsset.EnemyData[enemyTypeId].DeathEffect;
        if (deathEffect != null)
        {
            var effectInstance = Instantiate(deathEffect, instance.Position, Quaternion.identity);
            Destroy(effectInstance, 5);
        }
    }
    
    protected abstract Func<RenderGroup> GetGroupFactory();

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spawnBound);
    }
}
