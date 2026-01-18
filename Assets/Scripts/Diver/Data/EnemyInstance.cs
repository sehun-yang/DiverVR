using Unity.Mathematics;

public struct EnemyArchyType
{
    // Transform
    public float3 Position;
    public quaternion Rotation;
    public float Scale;

    // Physics
    public float3 Velocity;
    public float3 Acceleration;

    // Game Logic
    public int EnemyTypeId;
    public uint SpawnerId;
    public float Health;
    
    // Rendering
    public float BoundingRadius;
    public byte IsVisible;
    
    // Animation
    public float AnimationTime;
    public byte AnimationIndex;
    public float AnimationLength;
}
