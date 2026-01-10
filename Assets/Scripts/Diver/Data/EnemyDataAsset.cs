using System;
using UnityEngine;

[Serializable]
public class EnemyData
{
    public BakedAnimationAsset AnimationAsset;
    public Mesh Mesh;
    public Material Material;
}

[CreateAssetMenu(fileName = "EnemyDataAsset", menuName = "Data/EnemyDataAsset")]
public class EnemyDataAsset : ScriptableObject
{
    public EnemyData[] EnemyData;
}
