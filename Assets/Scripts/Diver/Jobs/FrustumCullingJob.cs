using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct FrustumCullingJob : IJobParallelFor
{
    public NativeArray<EnemyInstance> Enemies;
    [ReadOnly] public NativeArray<float4> FrustumPlanes;

    public void Execute(int index)
    {
        var enemy = Enemies[index];
#if UNITY_EDITOR
        enemy.IsVisible = (byte)1;
#else
        enemy.IsVisible = IsInFrustum(enemy.Position, enemy.BoundingRadius) ? (byte)1 : (byte)0;
#endif
        Enemies[index] = enemy;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsInFrustum(float3 center, float radius)
    {
        for (int i = 0; i < 6; i++)
        {
            float dist = math.dot(FrustumPlanes[i].xyz, center) + FrustumPlanes[i].w;
            if (dist < -radius)
                return false;
        }
        return true;
    }
}
