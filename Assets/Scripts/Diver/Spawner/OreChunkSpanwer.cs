using UnityEngine;

public class OreChunkSpawner : EnemySpawnerBase
{
    [SerializeField] private OreSpawner oreSpawner;
    private readonly RaycastHit[] raycastHits = new RaycastHit[1];
    private int searchIndex = 0;

    private const int MaxTryCount = 100;

    private void Start()
    {
        if (EnemyManager.Instance == null)
        {
            Debug.LogError("EnemyManager not found!");
            return;
        }

        var group = new OreChunkGroup(oreSpawner, groupId, enemyTypeId);
        groupId = EnemyManager.Instance.RegisterRenderGroup(group);
    }
    
    protected override (Vector3, Quaternion) GetSpawnPosition()
    {
        for (int i = 0; i < MaxTryCount; i++)
        {
            Vector3 direction = GetHaltonDirection(searchIndex++);
            
            if (Physics.RaycastNonAlloc(transform.position, direction, raycastHits, spawnBound, 1 << 3) > 0)
            {
                return (raycastHits[0].point, Quaternion.LookRotation(raycastHits[0].normal));
            }
        }

        Debug.LogWarning($"Cannot find spawn position for enemy #{enemyTypeId}");
        return (transform.position, Quaternion.identity);
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
}
