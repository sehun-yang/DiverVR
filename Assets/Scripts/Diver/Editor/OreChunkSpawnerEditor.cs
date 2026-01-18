using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(OreChunkSpawner))]
public class OreChunkSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        var spawner = (OreChunkSpawner)target;
        
        if (Application.isPlaying && GUILayout.Button("Kill all"))
        {
            spawner.DamageAll();
        }
    }
}
