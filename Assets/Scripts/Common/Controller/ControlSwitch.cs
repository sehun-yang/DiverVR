using UnityEngine;

public class ControlSwitch : MonoBehaviour
{
    private LocomotionControl currentControlInternal;
    private LocomotionControl currentControl
    {
        set
        {
            if (value != currentControlInternal)
            {
                currentControlInternal = value;
                RelativePositionControl.Instance.ChangeControl(value);
            }
        }
    }

    private void Update()
    {
        var myCharacter = CharacterSpawnSystem.Instance.myCharacter;
        if (myCharacter != null)
        {
            if (myCharacter.transform.position.y > 0)
            {
                currentControl = LocomotionControl.Default;
            }
            else
            {
                currentControl = LocomotionControl.Swim;
            }
        }
    }
}