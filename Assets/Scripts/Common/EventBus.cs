
using System;
using Fusion;
using UnityEngine;

public static class EventBus
{
    public static event Action<DiverActionType, bool> OnButtonPerformed;
    public static void ButtonPerformed(DiverActionType actionType, bool pressed) => OnButtonPerformed?.Invoke(actionType, pressed);
}