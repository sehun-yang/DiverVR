using System.Collections.Generic;
using UnityEngine;

public class ComputeBufferContainer : SingletonMonoBehaviourAutoCreate<ComputeBufferContainer>
{
    public ComputeBuffer EnemyRenderSystemAnimationTimeBuffer;
    public Dictionary<int, ComputeBuffer> EnemyRenderSystemBoneWeightBuffer = new();
    public Dictionary<int, ComputeBuffer> EnemyRenderSystemClipInfoBuffer = new();

    protected override void OnDestroy()
    {
        base.OnDestroy();

        EnemyRenderSystemAnimationTimeBuffer?.Release();
        
        foreach (var kv in EnemyRenderSystemBoneWeightBuffer)
        {
            kv.Value.Release();
        }
        
        foreach (var kv in EnemyRenderSystemClipInfoBuffer)
        {
            kv.Value.Release();
        }
    }
}