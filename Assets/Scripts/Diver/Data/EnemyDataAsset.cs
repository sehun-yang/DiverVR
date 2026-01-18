using System;
using UnityEngine;

[Serializable]
public class EnemyData
{
    public bool AnimationGPUSkinning;
    public BakedAnimationAsset AnimationAsset;
    public Vector3 Pivot = Vector3.zero;
    public Quaternion BaseRotation = Quaternion.identity;
    public Vector3 RandomRotationMask = Vector3.zero;
    public Mesh Mesh;
    public Material Material;
    public GameObject DeathEffect;
}

[CreateAssetMenu(fileName = "EnemyDataAsset", menuName = "Data/EnemyDataAsset")]
public class EnemyDataAsset : ScriptableObject
{
    public EnemyData[] EnemyData;
}
