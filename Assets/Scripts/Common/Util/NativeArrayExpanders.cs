// Auto-generated code
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

public static class NativeArrayExpanders
{
    public static NativeArray<Matrix4x4> ExpandNativeArray(this NativeArray<Matrix4x4> old, int newCapacity)
    {
        var newArray = new NativeArray<Matrix4x4>(newCapacity, Allocator.Persistent);
        if (old.IsCreated && old.Length > 0)
        {
            NativeArray<Matrix4x4>.Copy(old, newArray, math.min(old.Length, newCapacity));
            old.Dispose();
        }
        return newArray;
    }
    public static NativeArray<float3> ExpandNativeArray(this NativeArray<float3> old, int newCapacity)
    {
        var newArray = new NativeArray<float3>(newCapacity, Allocator.Persistent);
        if (old.IsCreated && old.Length > 0)
        {
            NativeArray<float3>.Copy(old, newArray, math.min(old.Length, newCapacity));
            old.Dispose();
        }
        return newArray;
    }
    public static NativeArray<float2> ExpandNativeArray(this NativeArray<float2> old, int newCapacity)
    {
        var newArray = new NativeArray<float2>(newCapacity, Allocator.Persistent);
        if (old.IsCreated && old.Length > 0)
        {
            NativeArray<float2>.Copy(old, newArray, math.min(old.Length, newCapacity));
            old.Dispose();
        }
        return newArray;
    }
    public static NativeArray<int> ExpandNativeArray(this NativeArray<int> old, int newCapacity)
    {
        var newArray = new NativeArray<int>(newCapacity, Allocator.Persistent);
        if (old.IsCreated && old.Length > 0)
        {
            NativeArray<int>.Copy(old, newArray, math.min(old.Length, newCapacity));
            old.Dispose();
        }
        return newArray;
    }
    public static NativeArray<float> ExpandNativeArray(this NativeArray<float> old, int newCapacity)
    {
        var newArray = new NativeArray<float>(newCapacity, Allocator.Persistent);
        if (old.IsCreated && old.Length > 0)
        {
            NativeArray<float>.Copy(old, newArray, math.min(old.Length, newCapacity));
            old.Dispose();
        }
        return newArray;
    }
    public static NativeArray<EnemyArcheType> ExpandNativeArray(this NativeArray<EnemyArcheType> old, int newCapacity)
    {
        var newArray = new NativeArray<EnemyArcheType>(newCapacity, Allocator.Persistent);
        if (old.IsCreated && old.Length > 0)
        {
            NativeArray<EnemyArcheType>.Copy(old, newArray, math.min(old.Length, newCapacity));
            old.Dispose();
        }
        return newArray;
    }
}
