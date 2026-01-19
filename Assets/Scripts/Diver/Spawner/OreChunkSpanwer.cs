using System;
using UnityEngine;

public class OreChunkSpawner : EnemySpawnerBase
{
    [Header("Ore Settings")]
    [SerializeField] private int oreId;
    [SerializeField] private int orePerChunk;

    private readonly RaycastHit[] raycastHits = new RaycastHit[1];
    private int searchIndex = 0;

    private const int MaxTryCount = 100;

    protected override void Start()
    {
        base.Start();

        var oreGroup = EnemyManager.Instance.GetOrAddRenderGroup(() => new InhaleBaseGroup(oreId), oreId);
    }
    
    protected override Func<RenderGroup> GetGroupFactory()
    {
        return () => new OreChunkGroup(enemyTypeId);
    }
    
    protected override void OnOneRemoved(int entityId)
    {
        base.OnOneRemoved(entityId);

        var instance = group.DataContainer.EnemyArcheTypeArray[entityId];
        var enemyData = EnemyManager.Instance.enemyDataAsset.EnemyData[enemyTypeId];
        SpawnNAt(oreId, orePerChunk, instance.Position, Quaternion.Euler(enemyData.RandomRotationMask.x * (UnityEngine.Random.value - 0.5f) * 2, enemyData.RandomRotationMask.y * (UnityEngine.Random.value - 0.5f) * 2, enemyData.RandomRotationMask.z * (UnityEngine.Random.value - 0.5f) * 2), 1);
    }
    
    protected override (Vector3, Quaternion, float) GetSpawnTRS()
    {
        for (int i = 0; i < MaxTryCount; i++)
        {
            Vector3 direction = GetHaltonDirection(searchIndex++);
            
            if (Physics.RaycastNonAlloc(transform.position, direction, raycastHits, spawnBound, 1 << 3) > 0)
            {
                return (raycastHits[0].point, Quaternion.LookRotation(raycastHits[0].normal), 0);
            }
        }

        Debug.LogWarning($"Cannot find spawn position for enemy #{enemyTypeId}");
        return (transform.position, Quaternion.identity, 0);
    }

    private static Vector3 GetHaltonDirection(int index)
    {
        float u = Halton(index + 1, 2);
        float v = Halton(index + 1, 3);
        
        float theta = 2f * Mathf.PI * u;
        float phi = Mathf.Acos(2f * v - 1f);
        
        return new Vector3(
            Mathf.Sin(phi) * Mathf.Cos(theta),
            Mathf.Cos(phi),
            Mathf.Sin(phi) * Mathf.Sin(theta)
        );
    }

    private static float Halton(int index, int baseNum)
    {
        float result = 0f;
        float f = 1f;
        while (index > 0)
        {
            f /= baseNum;
            result += f * (index % baseNum);
            index /= baseNum;
        }
        return result;
    }

#if UNITY_EDITOR
    public void DamageAll()
    {
        for (int i = 0; i < group.Count; i++)
        {
            var enemy = group.DataContainer.EnemyArcheTypeArray[i];
            if (enemy.SpawnerId != spawnerId) continue;
            enemy.Health = 0;
            group.DataContainer.EnemyArcheTypeArray[i] = enemy;
        }
    }
#endif
}
