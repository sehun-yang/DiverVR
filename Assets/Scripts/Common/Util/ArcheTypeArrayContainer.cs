// Auto-generated code
using Unity.Collections;

public class ArcheTypeArrayContainer
{
    public NativeArray<EnemyArcheType> EnemyArcheTypeArray;
    public void PullArray(int i0, int i1)
    {
        if (EnemyArcheTypeArray.IsCreated) EnemyArcheTypeArray[i0] = EnemyArcheTypeArray[i1];
    }
    public void ExpandArray(int newCapacity)
    {
        if (EnemyArcheTypeArray.IsCreated) EnemyArcheTypeArray = EnemyArcheTypeArray.ExpandNativeArray(newCapacity);
    }
    public void SetEntityData(ref EnemyArcheType data, int index)
    {
        EnemyArcheTypeArray[index] = data;
    }

    public void ClearAll()
    {
        if (EnemyArcheTypeArray.IsCreated) EnemyArcheTypeArray.Dispose();
    }
}
