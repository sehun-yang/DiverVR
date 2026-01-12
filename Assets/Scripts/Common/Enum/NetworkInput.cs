using Fusion;
using UnityEngine;

public struct NetworkInput : INetworkInput
{
    public Vector3 Position;
    public HalfQuaternion Rotation;

    public Vector3 LHPosition;
    public HalfQuaternion LHRotation;

    public Vector3 RHPosition;
    public HalfQuaternion RHRotation;

    public NetworkBitArraySingleByte HandFoldStatus;
}