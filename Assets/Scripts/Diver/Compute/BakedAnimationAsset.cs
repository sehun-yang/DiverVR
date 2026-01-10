using System;
using UnityEngine;

[Serializable]
public struct AnimationClipInfo
{
    public int startFrame;
    public int frameCount;
    public float duration;
}

public class BakedAnimationAsset : ScriptableObject
{
    public Texture2D atlasTexture;
    public int boneCount;
    public AnimationClipInfo[] clips;
}