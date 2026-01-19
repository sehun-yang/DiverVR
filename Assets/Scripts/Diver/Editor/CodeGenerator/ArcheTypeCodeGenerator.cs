#if UNITY_EDITOR
using UnityEditor;
using System.IO;
using System.Text;
using System;
using System.Linq;

public class ArcheTypeCodeGenerator
{
    [MenuItem("Tools/Generate Arche Type codes")]
    public static void GenerateCode()
    {
        GeneratedNativeArrayExpanderCodes();
        GenerateArcheTypeContainerCodes();

        AssetDatabase.Refresh();
        
        UnityEngine.Debug.Log($"Code generated");
    }

    private static void GenerateArcheTypeContainerCodes()
    {
        var enumTypes = Enum.GetNames(typeof(ArcheType));

        var sb = new StringBuilder();
        sb.AppendLine("// Auto-generated code");
        sb.AppendLine("using Unity.Collections;");
        sb.AppendLine();
        sb.AppendLine("public class ArcheTypeArrayContainer");
        sb.AppendLine("{");

        foreach (var type in enumTypes)
        {
        sb.AppendLine($@"    public NativeArray<{type}> {type}Array;");
        }
        sb.AppendLine($@"    public void PullArray(int i0, int i1)");
        sb.AppendLine(  "    {");
        foreach (var type in enumTypes)
        {
        sb.AppendLine($@"        if ({type}Array.IsCreated) {type}Array[i0] = {type}Array[i1];");
        }
        sb.AppendLine(  "    }");
        
        sb.AppendLine($@"    public void ExpandArray(int newCapacity)");
        sb.AppendLine(  "    {");
        foreach (var type in enumTypes)
        {
        sb.AppendLine($@"        if ({type}Array.IsCreated) {type}Array = {type}Array.ExpandNativeArray(newCapacity);");
        }
        sb.AppendLine(  "    }");
        
        foreach (var type in enumTypes)
        {
        sb.AppendLine($@"    public void SetEntityData(ref {type} data, int index)");
        sb.AppendLine(  "    {");
        sb.AppendLine($@"        {type}Array[index] = data;");
        sb.AppendLine(  "    }");
        }
        
        sb.AppendLine($@"    public void ClearAll()");
        sb.AppendLine(  "    {");
        foreach (var type in enumTypes)
        {
        sb.AppendLine($@"        if ({type}Array.IsCreated) {type}Array.Dispose();");
        }
        sb.AppendLine(  "    }");

        sb.AppendLine("}");

        Directory.CreateDirectory(Path.GetDirectoryName("Assets/Scripts/Common/Util/ArcheTypeArrayContainer.cs"));
        File.WriteAllText("Assets/Scripts/Common/Util/ArcheTypeArrayContainer.cs", sb.ToString());
    }

    private static void GeneratedNativeArrayExpanderCodes()
    {
        var defaultTypes = new[] { "Matrix4x4", "float3", "float2", "int", "float" };
        var enumTypes = Enum.GetNames(typeof(ArcheType));
        var allTypes = defaultTypes.Concat(enumTypes).ToArray();
        
        var sb = new StringBuilder();
        sb.AppendLine("// Auto-generated code");
        sb.AppendLine("using UnityEngine;");
        sb.AppendLine("using Unity.Collections;");
        sb.AppendLine("using Unity.Mathematics;");
        sb.AppendLine();
        sb.AppendLine("public static class NativeArrayExpanders");
        sb.AppendLine("{");

        foreach (var type in allTypes)
        {
            sb.AppendLine($@"    public static NativeArray<{type}> ExpandNativeArray(this NativeArray<{type}> old, int newCapacity)
    {{
        var newArray = new NativeArray<{type}>(newCapacity, Allocator.Persistent);
        if (old.IsCreated && old.Length > 0)
        {{
            NativeArray<{type}>.Copy(old, newArray, math.min(old.Length, newCapacity));
            old.Dispose();
        }}
        return newArray;
    }}");
        }

        sb.AppendLine("}");

        Directory.CreateDirectory(Path.GetDirectoryName("Assets/Scripts/Common/Util/NativeArrayExpanders.cs"));
        File.WriteAllText("Assets/Scripts/Common/Util/NativeArrayExpanders.cs", sb.ToString());
    }
}
#endif