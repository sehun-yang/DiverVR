using System;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class HierarchyColorizer
{
    static HierarchyColorizer()
    {
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
    }

    private static void OnHierarchyGUI(int instanceID, Rect selectionRect)
    {
        if (EditorUtility.EntityIdToObject(instanceID) is not GameObject obj) return;

        if (HasGenericBase(obj, typeof(SingletonMonoBehaviour<>)))
        {
            float indent = EditorGUI.indentLevel * 15f;
            Rect iconRect = new (
                selectionRect.x + indent,
                selectionRect.y,
                16f,
                selectionRect.height
            );

            Texture icon = EditorGUIUtility.ObjectContent(obj, typeof(GameObject)).image;

            if (icon != null)
            {
                Color prev = GUI.color;
                GUI.color = Color.red;

                GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);

                GUI.color = prev;
            }
        }
    }
    
    private static bool HasGenericBase(GameObject obj, Type openGenericType)
    {
        var components = obj.GetComponents<MonoBehaviour>();

        foreach (var comp in components)
        {
            if (comp == null) continue;

            Type type = comp.GetType();
            while (type != null)
            {
                if (type.IsGenericType &&
                    type.GetGenericTypeDefinition() == openGenericType)
                {
                    return true;
                }
                type = type.BaseType;
            }
        }
        return false;
    }

    private static bool HasDerivedComponent<T>(GameObject obj) where T : MonoBehaviour
    {
        var components = obj.GetComponents<MonoBehaviour>();
        foreach (var c in components)
        {
            if (c != null && c is T)
                return true;
        }
        return false;
    }
}