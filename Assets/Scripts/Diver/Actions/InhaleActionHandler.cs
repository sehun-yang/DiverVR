public class InhaleActionHandler : IActionPerformer
{
    public void HandleEvent(DiverActionType actionType, bool performed)
    {
        
    }

    public bool WillHandleEvent(DiverActionType actionType)
    {
        return actionType == DiverActionType.Inhale;
    }
}