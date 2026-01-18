using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public abstract class RenderGroup : IDisposable
{
    public int EnemyTypeId;

    public NativeArray<EnemyArcheType> Enemies;
    public NativeArray<Matrix4x4> Matrices;
    public NativeArray<float2> AnimationData;

    protected int currentCapacity;
    protected bool useAnimation;
    protected const int InitialCapacity = 64;
    protected const int CapacityGrowth = 64;

    public int Count = 0;

    public RenderGroup(int enemyTypeId)
    {
        EnemyTypeId = enemyTypeId;
        currentCapacity = InitialCapacity;
        Enemies = new NativeArray<EnemyArcheType>(currentCapacity, Allocator.Persistent);
        Matrices = new NativeArray<Matrix4x4>(currentCapacity, Allocator.Persistent);
    }

    public void EnsureCapacity(int required)
    {
        if (required <= currentCapacity) return;

        int newCapacity = ((required / CapacityGrowth) + 1) * CapacityGrowth;

        Enemies = ExpandNativeArray(Enemies, newCapacity);
        Matrices = ExpandNativeArray(Matrices, newCapacity);
        currentCapacity = newCapacity;

        if(useAnimation)
        {
            AnimationData = ExpandNativeArray(AnimationData, newCapacity);
        }
    }

    private NativeArray<T> ExpandNativeArray<T>(NativeArray<T> old, int newCapacity) where T : struct
    {
        var newMatrices = new NativeArray<T>(newCapacity, Allocator.Persistent);

        if (old.IsCreated && old.Length > 0)
        {
            NativeArray<T>.Copy(old, newMatrices, math.min(old.Length, newCapacity));
            old.Dispose();
        }

        return newMatrices;
    }

    public void AddEnemy(EnemyArcheType enemy)
    {
        EnsureCapacity(Count + 1);
        Enemies[Count] = enemy;
        Count++;
    }

    public void Dispose()
    {
        if (Enemies.IsCreated) Enemies.Dispose();
        if (Matrices.IsCreated) Matrices.Dispose();
        if (AnimationData.IsCreated) AnimationData.Dispose();
    }

    public virtual void Update(float deltaTime)
    {
        
    }
}
