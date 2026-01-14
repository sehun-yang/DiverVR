using Unity.Mathematics;

public struct EnemyInstance
{
    public float3 Position;
    public quaternion Rotation;
    public float3 Velocity;
    public float3 Acceleration;
    public int EnemyTypeId;
    public float AnimationTime;
    public int AnimationIndex;
    public float AnimationLength;
    public float BoundingRadius;
    public byte IsVisible;
}
