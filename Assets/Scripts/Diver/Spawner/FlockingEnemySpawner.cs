using UnityEngine;

public class FlockingEnemySpawner : EnemySpawnerBase
{
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

        var group = new FlockGroup(groupId, enemyTypeId, transform.position, spawnBound * 2f, settings);
        groupId = EnemyManager.Instance.RegisterRenderGroup(group);
    }
}
