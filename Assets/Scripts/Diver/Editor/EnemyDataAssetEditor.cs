using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EnemyDataAsset))]
public class EnemyDataAssetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        var asset = (EnemyDataAsset)target;
        
        GUILayout.Space(10);
        
        if (asset.EnemyData != null)
        {
            EditorGUILayout.LabelField("Enemy Data Summary", EditorStyles.boldLabel);
            
            for (int i = 0; i < asset.EnemyData.Length; i++)
            {
                var data = asset.EnemyData[i];
                string meshName = data.Mesh != null ? data.Mesh.name : "None";
                string animName = data.AnimationAsset != null ? data.AnimationAsset.name : "None";
                string matName = data.Material != null ? data.Material.name : "None";
                
                EditorGUILayout.LabelField($"[{i}] Mesh: {meshName}, Anim: {animName}, Mat: {matName}");
            }
        }
    }
}
