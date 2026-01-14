using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class EnemyManager : SingletonMonoBehaviour<EnemyManager>
{
    [Header("Settings")]
    public EnemyDataAsset enemyDataAsset;
    public Camera renderCamera;

    private readonly Dictionary<int, RenderGroup> renderingGroups = new();
    private NativeArray<float4> frustumPlanes;
    private NativeArray<Plane> cameraPlanes;
    private bool initialized;
    private int nextGroupId;

    private const int MaxBatchSize = 1023;

    private void Start()
    {
        if (renderCamera == null)
            renderCamera = Camera.main;

        frustumPlanes = new NativeArray<float4>(6, Allocator.Persistent);
        cameraPlanes = new NativeArray<Plane>(6, Allocator.Persistent);
        InitializeMaterials();
        initialized = true;
    }

    private bool OtherModuleLoaded()
    {
        return RelativePositionControl.Instance.started;
    }

    private void LateUpdate()
    {
        if (!initialized || renderingGroups.Count == 0) return;
        if (!OtherModuleLoaded()) return;

        float deltaTime = Time.deltaTime;

        foreach (var kvp in renderingGroups)
        {
            var group = kvp.Value;
            if (group.Count == 0) continue;

            group.UpdateGroup(deltaTime);
        }

        ExtractFrustumPlanes();

        foreach (var kvp in renderingGroups)
        {
            var group = kvp.Value;
            if (group.Count == 0) continue;

            CullAndRenderGroup(group);
        }
    }

    private void CullAndRenderGroup(RenderGroup group)
    {
        int count = group.Count;
        if (count == 0) return;

        var cullingJob = new FrustumCullingJob
        {
            Enemies = group.Enemies.AsArray(),
            FrustumPlanes = frustumPlanes
        };
        cullingJob.Schedule(count, 64).Complete();

        var enemyData = enemyDataAsset.EnemyData[group.EnemyTypeId];
        group.EnsureCapacity(count);

        var visibleCountRef = new NativeReference<int>(Allocator.TempJob);


        if (enemyData.GPUSkinning)
        {
            var collectJob = new CollectRenderDataJob
            {
                Enemies = group.Enemies.AsArray(),
                Matrices = group.Matrices,
                AnimationData = group.AnimationData,
                VisibleCount = visibleCountRef
            };
            collectJob.Schedule().Complete();
        }
        else
        {
            var collectJob = new CollectRenderDataJobNoAnimation
            {
                Enemies = group.Enemies.AsArray(),
                Matrices = group.Matrices,
                VisibleCount = visibleCountRef
            };
            collectJob.Schedule().Complete();
        }

        int visibleCount = visibleCountRef.Value;
        visibleCountRef.Dispose();

        if (visibleCount == 0) return;


        if (enemyData.GPUSkinning)
        {
            ComputeBufferContainer.Instance.EnemyRenderSystemAnimationTimeBuffer.SetData(group.AnimationData, 0, 0, visibleCount);
        }

        var renderParams = new RenderParams(enemyData.Material)
        {
            worldBounds = new Bounds(Vector3.zero, Vector3.one * 10000f),
            shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off,
            receiveShadows = false
        };

        RenderMeshInstancedAll(ref renderParams, enemyData.Mesh, group.Matrices, visibleCount);
    }

    private void RenderMeshInstancedAll(ref RenderParams rp, Mesh mesh, NativeArray<Matrix4x4> matrices, int totalCount)
    {
        int offset = 0;

        while (offset < totalCount)
        {
            int batchSize = Mathf.Min(MaxBatchSize, totalCount - offset);
            Graphics.RenderMeshInstanced(rp, mesh, 0, matrices, batchSize, offset);
            offset += batchSize;
        }
    }

    private void ExtractFrustumPlanes()
    {
        GeometryUtility.CalculateFrustumPlanes(renderCamera, cameraPlanes.AsSpan());

        for (int i = 0; i < 6; i++)
        {
            frustumPlanes[i] = new float4(
                cameraPlanes[i].normal.x,
                cameraPlanes[i].normal.y,
                cameraPlanes[i].normal.z,
                cameraPlanes[i].distance
            );
        }
    }

    private void InitializeMaterials()
    {
        ComputeBufferContainer.Instance.EnemyRenderSystemAnimationTimeBuffer = new (1024, UnsafeUtility.SizeOf<float2>(), ComputeBufferType.Structured);

        for (int enemyId = 0; enemyId < enemyDataAsset.EnemyData.Length; enemyId++)
        {
            var data = enemyDataAsset.EnemyData[enemyId];
            if (!data.GPUSkinning) return;
            var material = data.Material;

            var boneWeightBuffer = CreateBoneWeightBuffer(data.Mesh);
            ComputeBufferContainer.Instance.EnemyRenderSystemBoneWeightBuffer[enemyId] = boneWeightBuffer;

            var clipInfoBuffer = CreateClipInfoBuffer(data.AnimationAsset);
            ComputeBufferContainer.Instance.EnemyRenderSystemClipInfoBuffer[enemyId] = clipInfoBuffer;

            material.SetBuffer("_BoneWeights", boneWeightBuffer);
            material.SetBuffer("_AnimationTimes", ComputeBufferContainer.Instance.EnemyRenderSystemAnimationTimeBuffer);
            material.SetBuffer("_ClipInfos", clipInfoBuffer);
            material.SetTexture("_AnimationTex", data.AnimationAsset.atlasTexture);
            material.SetInt("_BoneCount", data.AnimationAsset.boneCount);
        }
    }

    private ComputeBuffer CreateBoneWeightBuffer(Mesh mesh)
    {
        var bonesPerVertex = mesh.GetBonesPerVertex();
        var allBoneWeights = mesh.GetAllBoneWeights();
        int vertexCount = bonesPerVertex.Length;

        var gpuData = new NativeArray<GpuBoneWeightData>(vertexCount, Allocator.Temp);
        int weightIndex = 0;

        for (int vertexId = 0; vertexId < vertexCount; vertexId++)
        {
            int boneCount = bonesPerVertex[vertexId];

            var packed = new GpuBoneWeightData
            {
                BoneIndices = int4.zero,
                Weights = float4.zero
            };

            for (int i = 0; i < boneCount && i < 4; i++)
            {
                var boneWeight = allBoneWeights[weightIndex + i];
                packed.BoneIndices[i] = boneWeight.boneIndex;
                packed.Weights[i] = boneWeight.weight;
            }

            weightIndex += boneCount;
            gpuData[vertexId] = packed;
        }

        var buffer = new ComputeBuffer(gpuData.Length, UnsafeUtility.SizeOf<GpuBoneWeightData>(), ComputeBufferType.Structured);
        buffer.SetData(gpuData);
        gpuData.Dispose();

        return buffer;
    }

    private ComputeBuffer CreateClipInfoBuffer(BakedAnimationAsset animAsset)
    {
        var clips = animAsset.clips;
        var gpuClipInfos = new GpuAnimationClipInfo[clips.Length];

        for (int i = 0; i < clips.Length; i++)
        {
            gpuClipInfos[i] = new GpuAnimationClipInfo
            {
                StartFrame = clips[i].startFrame,
                FrameCount = clips[i].frameCount,
                Duration = clips[i].duration
            };
        }

        var buffer = new ComputeBuffer(gpuClipInfos.Length, UnsafeUtility.SizeOf<GpuAnimationClipInfo>(), ComputeBufferType.Structured);
        buffer.SetData(gpuClipInfos);

        return buffer;
    }

    public int RegisterRenderGroup(RenderGroup group)
    {
        int groupId = nextGroupId++;
        renderingGroups[groupId] = group;
        return groupId;
    }
    
    public int GetCount(int groupId)
    {
        if (!renderingGroups.TryGetValue(groupId, out var group)) return int.MaxValue;

        return group.Enemies.Length;
    }

    public void SpawnEnemy(int groupId, Vector3 position)
    {
        if (!renderingGroups.TryGetValue(groupId, out var group)) return;

        var enemyData = enemyDataAsset.EnemyData[group.EnemyTypeId];

        var enemy = new EnemyInstance
        {
            Position = position,
            Rotation = quaternion.identity,
            Velocity = float3.zero,
            EnemyTypeId = group.EnemyTypeId,
            AnimationTime = 0,
            AnimationIndex = 0,
            AnimationLength = enemyData.GPUSkinning ? enemyData.AnimationAsset.clips[0].duration : 0,
            BoundingRadius = enemyData.Mesh.bounds.extents.magnitude,
            IsVisible = 1
        };

        group.AddEnemy(enemy);
    }

    public void RemoveRenderGroup(int groupId)
    {
        if (renderingGroups.TryGetValue(groupId, out var group))
        {
            group.Dispose();
            renderingGroups.Remove(groupId);
        }
    }

    public RenderGroup GetRenderGroup(int groupId)
    {
        renderingGroups.TryGetValue(groupId, out var group);
        return group;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        foreach (var kvp in renderingGroups)
        {
            kvp.Value.Dispose();
        }
        renderingGroups.Clear();

        if (frustumPlanes.IsCreated) frustumPlanes.Dispose();
        if (cameraPlanes.IsCreated) cameraPlanes.Dispose();
    }
}