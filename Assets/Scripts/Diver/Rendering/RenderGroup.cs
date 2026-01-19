using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public abstract class RenderGroup : IDisposable
{
    public int EnemyTypeId;

    public ArcheTypeArrayContainer DataContainer = new ();
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
        DataContainer.EnemyArcheTypeArray = new NativeArray<EnemyArcheType>(currentCapacity, Allocator.Persistent);
        Matrices = new NativeArray<Matrix4x4>(currentCapacity, Allocator.Persistent);
    }

    public void EnsureCapacity(int required)
    {
        if (required <= currentCapacity) return;

        int newCapacity = ((required / CapacityGrowth) + 1) * CapacityGrowth;

        DataContainer.ExpandArray(newCapacity);
        Matrices = Matrices.ExpandNativeArray(newCapacity);
        currentCapacity = newCapacity;

        if(useAnimation)
        {
            AnimationData = AnimationData.ExpandNativeArray(newCapacity);
        }
    }

    public int AddEnemy()
    {
        int newIndex = Count;
        EnsureCapacity(newIndex + 1);
        Count++;

        return newIndex;
    }

    public void Dispose()
    {
        DataContainer.ClearAll();
        if (Matrices.IsCreated) Matrices.Dispose();
        if (AnimationData.IsCreated) AnimationData.Dispose();
    }

    public virtual void Update(float deltaTime)
    {
        
    }
}
