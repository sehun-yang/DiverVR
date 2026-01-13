using System.Collections.Generic;

public class ActionManager : SingletonMonoBehaviour<ActionManager>
{
    private List<IActionPerformer> handlers = new ();

    private void Start()
    {
        EventBus.OnButtonPerformed += OnButtonPerformed;

        handlers.Add(new InhaleActionHandler());
    }

    private void OnButtonPerformed(DiverActionType actionType, bool performed)
    {
        foreach (var handler in handlers)
        {
            if (handler.WillHandleEvent(actionType))
            {
                handler.HandleEvent(actionType, performed);
            }
        }
    }
}