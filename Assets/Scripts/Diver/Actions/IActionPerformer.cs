public interface IActionPerformer
{
    bool WillHandleEvent(DiverActionType actionType);
    void HandleEvent(DiverActionType actionType, bool performed);
}