using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct CollectRenderDataJobNoAnimation : IJob
{
    [ReadOnly] public NativeArray<EnemyArcheType> Enemies;
    [ReadOnly] public int MaxCount;
    [WriteOnly] public NativeArray<Matrix4x4> Matrices;
    public NativeReference<int> VisibleCount;

    public void Execute()
    {
        int count = 0;
        
        for (int i = 0; i < MaxCount; i++)
        {
            var enemy = Enemies[i];
            
            if (enemy.IsVisible == 0) continue;
            
            Matrices[count] = float4x4.TRS(enemy.Position, enemy.Rotation, new float3(enemy.Scale, enemy.Scale, enemy.Scale));
            count++;
        }
        
        VisibleCount.Value = count;
    }
}
