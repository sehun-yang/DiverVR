using System;
using Fusion;
using UnityEngine;

[Serializable]
public readonly struct HalfQuaternion : INetworkStruct
{
    public readonly Half x;
    public readonly Half y;
    public readonly Half z;

    public HalfQuaternion(Quaternion q)
    {
        if (q.w < 0)
        {
            q = new Quaternion(-q.x, -q.y, -q.z, -q.w);
        }

        x = (Half)q.x;
        y = (Half)q.y;
        z = (Half)q.z;
    }

    public readonly Quaternion ToQuaternion()
    {
        float fx = (float)x;
        float fy = (float)y;
        float fz = (float)z;
        
        float sumSquares = fx * fx + fy * fy + fz * fz;
        float fw = sumSquares >= 1.0f ? 0.0f : Mathf.Sqrt(1.0f - sumSquares);
        
        return new Quaternion(fx, fy, fz, fw);
    }

    public static implicit operator HalfQuaternion(Quaternion q)
    {
        return new HalfQuaternion(q);
    }

    public static implicit operator Quaternion(HalfQuaternion hq)
    {
        return hq.ToQuaternion();
    }
}