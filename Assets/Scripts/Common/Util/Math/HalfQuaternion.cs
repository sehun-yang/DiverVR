using System;
using Fusion;
using UnityEngine;

[Serializable]
public readonly struct HalfQuaternion : INetworkStruct
{
    public readonly Half x;
    public readonly Half y;
    public readonly Half z;
    public readonly Half w;

    public HalfQuaternion(Quaternion q)
    {
        x = (Half)q.x;
        y = (Half)q.y;
        z = (Half)q.z;
        w = (Half)q.w;
    }

    public readonly Quaternion ToQuaternion()
    {
        return new Quaternion((float)x, (float)y, (float)z, (float)w);
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