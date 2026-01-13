using UnityEngine;

public class InhaleActionHandler : IActionPerformer
{
    public void HandleEvent(DiverActionType actionType, bool performed)
    {
        ModuleManager.Instance.InhaleModule.Enabled = performed;
    }

    public bool WillHandleEvent(DiverActionType actionType)
    {
        return actionType == DiverActionType.Inhale;
    }
}