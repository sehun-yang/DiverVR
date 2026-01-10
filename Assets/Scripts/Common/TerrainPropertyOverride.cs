using UnityEngine;

public class TerrainPropertyOverride : MonoBehaviour
{
    public bool PlatformCanMove = false;
    public float MaxClimbAngle = 180;
    public float ContactBreakDistance = 0.9f;
    public float ContactBreakDistanceNormalDirection = 0.016f;
    public float ContactBreakSpeed = 3.5f;
    public float ShootSpeed = 2f;
}