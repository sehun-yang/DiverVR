using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public int enemyTypeId;
    public int maxSpawnAmount = 10;
    public float spawnBound = 5f;
    public bool autoSpawn = true;

    [Header("Flock Settings")]
    public float neighborDistance = 3f;
    public float separationDistance = 1f;
    public float separationWeight = 1.5f;
    public float alignmentWeight = 1f;
    public float cohesionWeight = 1f;
    public float returnWeight = 2f;
    public float minSpeed = 1f;
    public float maxSpeed = 5f;
    public float rotationSpeed = 5f;

    private int groupId = -1;
    private int currentSpawnCount;

    private void Start()
    {
        if (EnemyManager.Instance == null)
        {
            Debug.LogError("EnemyManager not found!");
            return;
        }

        var settings = new FlockSettings
        {
            NeighborDistance = neighborDistance,
            SeparationDistance = separationDistance,
            SeparationWeight = separationWeight,
            AlignmentWeight = alignmentWeight,
            CohesionWeight = cohesionWeight,
            ReturnWeight = returnWeight,
            MinSpeed = minSpeed,
            MaxSpeed = maxSpeed,
            RotationSpeed = rotationSpeed
        };

        groupId = EnemyManager.Instance.CreateFlockGroup(
            enemyTypeId,
            transform.position,
            spawnBound * 2f,
            settings
        );
    }

    private void Update()
    {
        if (autoSpawn)
        {
            SpawnAll();
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
        
        var group = EnemyManager.Instance.GetFlockGroup(groupId);
        if (group == null || group.Count == 0) return;

        EnemyManager.Instance.RemoveEnemy(groupId, group.Count - 1);
        currentSpawnCount--;
    }

    public void DespawnAll()
    {
        if (groupId < 0 || EnemyManager.Instance == null) return;

        var group = EnemyManager.Instance.GetFlockGroup(groupId);
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
            EnemyManager.Instance.RemoveFlockGroup(groupId);
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
