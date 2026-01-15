using UnityEngine;

public class ControlSwitch : MonoBehaviour
{
    [SerializeField] private float FlyingMinimumHeight = 4.0f;

    private readonly RaycastHit[] hits = new RaycastHit[1];

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
                if (Physics.RaycastNonAlloc(RigControl.Instance.transform.position, -Vector3.up, hits, FlyingMinimumHeight, 1 << 3) == 0)
                {
                    currentControl = LocomotionControl.Flying;
                }
                else
                {
                    currentControl = LocomotionControl.Default;
                }
            }
            else
            {
                currentControl = LocomotionControl.Swim;
            }
        }
    }
}