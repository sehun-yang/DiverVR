using System;

[Serializable]
public struct FlockSettings
{
    public float NeighborDistance;
    public float SeparationDistance;
    public float SeparationWeight;
    public float AlignmentWeight;
    public float CohesionWeight;
    public float ReturnWeight;
    public float MinSpeed;
    public float MaxSpeed;
    public float RotationSpeed;

    public static FlockSettings Default => new FlockSettings
    {
        NeighborDistance = 3f,
        SeparationDistance = 1f,
        SeparationWeight = 1.5f,
        AlignmentWeight = 1f,
        CohesionWeight = 1f,
        ReturnWeight = 2f,
        MinSpeed = 1f,
        MaxSpeed = 5f,
        RotationSpeed = 5f
    };
}
