using UnityEngine;

public class PMMath
{
    public static void GetCapsulePoints(CapsuleCollider capsule, out Vector3 point0, out Vector3 point1)
    {
        Transform t = capsule.transform;

        Vector3 center = t.TransformPoint(capsule.center);

        float scaleY = Mathf.Abs(t.lossyScale.y);
        float scaleX = Mathf.Abs(t.lossyScale.x);
        float scaleZ = Mathf.Abs(t.lossyScale.z);

        Vector3 dir;
        float heightScale;
        switch (capsule.direction)
        {
            case 0:
                dir = t.right;
                heightScale = scaleX;
                break;
            case 1:
                dir = t.up;
                heightScale = scaleY;
                break;
            case 2:
                dir = t.forward;
                heightScale = scaleZ;
                break;
            default:
                dir = t.up;
                heightScale = scaleY;
                break;
        }

        float radius = capsule.radius * Mathf.Max(scaleX, scaleY, scaleZ);
        float height = capsule.height * heightScale;

        float halfLine = Mathf.Max(0f, (height * 0.5f) - radius) * 0.99f; // 0.99 for padding

        point0 = center + dir * halfLine;
        point1 = center - dir * halfLine;
    }
}