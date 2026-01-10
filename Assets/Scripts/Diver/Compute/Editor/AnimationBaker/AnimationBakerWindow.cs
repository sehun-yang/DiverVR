using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class AnimationBakerWindow : EditorWindow
{
    private GameObject targetPrefab;
    private readonly List<AnimationClip> clips = new ();
    private float frameRate = 30f;
    private string savePath = "Assets/BakedAnimations";
    private string assetName = "NewBakedAnimation";

    [MenuItem("Tools/Animation Baker")]
    public static void ShowWindow()
    {
        GetWindow<AnimationBakerWindow>("Animation Baker");
    }

    private void OnGUI()
    {
        GUILayout.Label("Animation Atlas Baker", EditorStyles.boldLabel);
        
        targetPrefab = (GameObject)EditorGUILayout.ObjectField("Target Prefab", targetPrefab, typeof(GameObject), false);
        savePath = EditorGUILayout.TextField("Save Path", savePath);
        assetName = EditorGUILayout.TextField("Asset Name", assetName);
        
        GUILayout.Space(10);
        GUILayout.Label("Animation Clips:", EditorStyles.boldLabel);
        
        for (int i = 0; i < clips.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            clips[i] = (AnimationClip)EditorGUILayout.ObjectField($"Clip {i}", clips[i], typeof(AnimationClip), false);
            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                clips.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndHorizontal();
        }
        
        if (GUILayout.Button("Add Clip"))
        {
            clips.Add(null);
        }

        GUILayout.Space(10);
        
        if (GUILayout.Button("Bake Atlas"))
        {
            if (targetPrefab != null && clips.Count > 0 && clips.TrueForAll(c => c != null))
            {
                BakeAtlas();
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Prefab과 모든 Animation Clip을 설정해주세요.", "OK");
            }
        }
    }

    private void BakeAtlas()
    {
        var instance = Instantiate(targetPrefab, Vector3.zero, Quaternion.identity);
        instance.transform.localScale = Vector3.one;
        
        var skinnedMeshRenderer = instance.GetComponentInChildren<SkinnedMeshRenderer>();
        
        if (skinnedMeshRenderer == null)
        {
            DestroyImmediate(instance);
            EditorUtility.DisplayDialog("Error", "SkinnedMeshRenderer를 찾을 수 없습니다.", "OK");
            return;
        }

        var bones = skinnedMeshRenderer.bones;
        var bindPoses = skinnedMeshRenderer.sharedMesh.bindposes;
        var meshTransform = skinnedMeshRenderer.transform;
        int boneCount = bones.Length;
        
        List<AnimationClipInfo> clipInfos = new ();
        int totalFrames = 0;
        
        foreach (var clip in clips)
        {
            int clipFrameCount = Mathf.CeilToInt(clip.length * frameRate) + 1;
            
            clipInfos.Add(new AnimationClipInfo
            {
                startFrame = totalFrames,
                frameCount = clipFrameCount,
                duration = clip.length
            });
            
            totalFrames += clipFrameCount;
        }

        var atlasTexture = new Texture2D(boneCount * 4, totalFrames, TextureFormat.RGBAFloat, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        for (int clipIndex = 0; clipIndex < clips.Count; clipIndex++)
        {
            var clip = clips[clipIndex];
            var clipInfo = clipInfos[clipIndex];
            
            for (int frame = 0; frame < clipInfo.frameCount; frame++)
            {
                float time = frame / frameRate;
                clip.SampleAnimation(instance, time);

                Matrix4x4 rootInverse = meshTransform.worldToLocalMatrix;

                for (int boneIdx = 0; boneIdx < boneCount; boneIdx++)
                {
                    Matrix4x4 boneMatrix = rootInverse * bones[boneIdx].localToWorldMatrix * bindPoses[boneIdx];

                    for (int col = 0; col < 4; col++)
                    {
                        int x = boneIdx * 4 + col;
                        int y = clipInfo.startFrame + frame;
                        
                        Color pixel = new Color(
                            boneMatrix[0, col],
                            boneMatrix[1, col],
                            boneMatrix[2, col],
                            boneMatrix[3, col]
                        );
                        
                        atlasTexture.SetPixel(x, y, pixel);
                    }
                }
            }
        }

        atlasTexture.Apply();
        DestroyImmediate(instance);

        if (!AssetDatabase.IsValidFolder(savePath))
        {
            System.IO.Directory.CreateDirectory(savePath);
            AssetDatabase.Refresh();
        }

        string texturePath = $"{savePath}/{assetName}_Atlas.asset";
        string dataPath = $"{savePath}/{assetName}.asset";

        AssetDatabase.CreateAsset(atlasTexture, texturePath);

        var bakedAsset = CreateInstance<BakedAnimationAsset>();
        bakedAsset.atlasTexture = atlasTexture;
        bakedAsset.boneCount = boneCount;
        bakedAsset.clips = clipInfos.ToArray();

        AssetDatabase.CreateAsset(bakedAsset, dataPath);
        AssetDatabase.SaveAssets();

        string clipList = "";
        for (int i = 0; i < clipInfos.Count; i++)
        {
            clipList += $"\n  [{i}] : frames {clipInfos[i].startFrame}-{clipInfos[i].startFrame + clipInfos[i].frameCount - 1}";
        }
        
        EditorUtility.DisplayDialog("Success", $"Baked {clips.Count} clips\nAtlas size: {atlasTexture.width}x{atlasTexture.height}{clipList}", "OK");
    }
}