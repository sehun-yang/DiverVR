using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public abstract class RenderGroup : IDisposable
{
    public int GroupId;
    public int EnemyTypeId;

    public NativeList<EnemyInstance> Enemies;
    public NativeArray<Matrix4x4> Matrices;
    public NativeArray<float2> AnimationData;

    protected int currentCapacity;
    protected bool useAnimation;
    protected const int InitialCapacity = 64;
    protected const int CapacityGrowth = 64;

    public int Count => Enemies.Length;

    public void EnsureCapacity(int required)
    {
        if (required <= currentCapacity) return;

        int newCapacity = ((required / CapacityGrowth) + 1) * CapacityGrowth;

        var newMatrices = new NativeArray<Matrix4x4>(newCapacity, Allocator.Persistent);

        if (Matrices.IsCreated && Matrices.Length > 0)
        {
            NativeArray<Matrix4x4>.Copy(Matrices, newMatrices, math.min(Matrices.Length, newCapacity));
            Matrices.Dispose();
        }

        Matrices = newMatrices;
        currentCapacity = newCapacity;

        if(useAnimation)
        {
            var newAnimData = new NativeArray<float2>(newCapacity, Allocator.Persistent);
            if (AnimationData.IsCreated && AnimationData.Length > 0)
            {
                NativeArray<float2>.Copy(AnimationData, newAnimData, math.min(AnimationData.Length, newCapacity));
                AnimationData.Dispose();
            }
            AnimationData = newAnimData;
        }
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
