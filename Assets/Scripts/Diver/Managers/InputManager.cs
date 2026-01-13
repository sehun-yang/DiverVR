using UnityEngine;
using UnityEngine.InputSystem;
using Fusion;

public class InputManager : SingletonMonoBehaviour<InputManager>
{
    [SerializeField] private InputActionAsset asset = null;

    [SerializeField] private SerializableDictionary<InputActionReference, DiverActionType> referenceMap = new();
    private readonly SerializableDictionary<InputAction, DiverActionType> actionMap = new();

    private void OnEnable()
    {
        asset.Enable();

        foreach (var pair in referenceMap)
        {
            pair.Key.action.performed += OnPerformed;
            pair.Key.action.canceled += OnCanceled;
            actionMap.Add(pair.Key.action, pair.Value);
        }
    }

    private void OnDisable()
    {
        foreach (var pair in referenceMap)
        {
            pair.Key.action.performed -= OnPerformed;
            pair.Key.action.canceled -= OnCanceled;
            actionMap.Clear();
        }

        asset.Disable();
    }

    private void OnPerformed(InputAction.CallbackContext context)
    {
        if (actionMap.TryGetValue(context.action, out var button))
        {
            EventBus.ButtonPerformed(button, true);
        }
    }

    private void OnCanceled(InputAction.CallbackContext context)
    {
        if (actionMap.TryGetValue(context.action, out var button))
        {
            EventBus.ButtonPerformed(button, false);
        }
    }
}