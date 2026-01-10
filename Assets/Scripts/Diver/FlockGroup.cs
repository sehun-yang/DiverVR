using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class FlockGroup : IDisposable
{
    public int GroupId;
    public int EnemyTypeId;
    public float3 OriginPoint;
    public float MaxDistanceSq;
    public FlockSettings Settings;

    public NativeList<EnemyInstance> Enemies;
    public NativeArray<Matrix4x4> Matrices;
    public NativeArray<float2> AnimationData;

    private int currentCapacity;
    private const int InitialCapacity = 64;
    private const int CapacityGrowth = 64;

    public int Count => Enemies.Length;

    public FlockGroup(int groupId, int enemyTypeId, float3 origin, float maxDistance, FlockSettings settings)
    {
        GroupId = groupId;
        EnemyTypeId = enemyTypeId;
        OriginPoint = origin;
        MaxDistanceSq = maxDistance * maxDistance;
        Settings = settings;

        currentCapacity = InitialCapacity;
        Enemies = new NativeList<EnemyInstance>(currentCapacity, Allocator.Persistent);
        Matrices = new NativeArray<Matrix4x4>(currentCapacity, Allocator.Persistent);
        AnimationData = new NativeArray<float2>(currentCapacity, Allocator.Persistent);
    }

    public void AddEnemy(EnemyInstance enemy)
    {
        Enemies.Add(enemy);
        EnsureCapacity(Enemies.Length);
    }

    public void RemoveAt(int index)
    {
        Enemies.RemoveAtSwapBack(index);
    }

    public void EnsureCapacity(int required)
    {
        if (required <= currentCapacity) return;

        int newCapacity = ((required / CapacityGrowth) + 1) * CapacityGrowth;

        var newMatrices = new NativeArray<Matrix4x4>(newCapacity, Allocator.Persistent);
        var newAnimData = new NativeArray<float2>(newCapacity, Allocator.Persistent);

        if (Matrices.IsCreated && Matrices.Length > 0)
        {
            NativeArray<Matrix4x4>.Copy(Matrices, newMatrices, math.min(Matrices.Length, newCapacity));
            Matrices.Dispose();
        }

        if (AnimationData.IsCreated && AnimationData.Length > 0)
        {
            NativeArray<float2>.Copy(AnimationData, newAnimData, math.min(AnimationData.Length, newCapacity));
            AnimationData.Dispose();
        }

        Matrices = newMatrices;
        AnimationData = newAnimData;
        currentCapacity = newCapacity;
    }

    public void Dispose()
    {
        if (Enemies.IsCreated) Enemies.Dispose();
        if (Matrices.IsCreated) Matrices.Dispose();
        if (AnimationData.IsCreated) AnimationData.Dispose();
    }
}
