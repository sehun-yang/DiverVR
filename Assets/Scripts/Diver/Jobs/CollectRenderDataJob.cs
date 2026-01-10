using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct CollectRenderDataJob : IJob
{
    [ReadOnly] public NativeArray<EnemyInstance> Enemies;
    [WriteOnly] public NativeArray<Matrix4x4> Matrices;
    [WriteOnly] public NativeArray<float2> AnimationData;
    public NativeReference<int> VisibleCount;

    public void Execute()
    {
        int count = 0;
        
        for (int i = 0; i < Enemies.Length; i++)
        {
            var enemy = Enemies[i];
            
            if (enemy.IsVisible == 0) continue;
            
            Matrices[count] = float4x4.TRS(enemy.Position, enemy.Rotation, new float3(1, 1, 1));
            AnimationData[count] = new float2(enemy.AnimationTime, enemy.AnimationIndex);
            count++;
        }
        
        VisibleCount.Value = count;
    }
}
