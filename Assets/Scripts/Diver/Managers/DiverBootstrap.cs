using UnityEngine;

public class DiverBootstrap : MonoBehaviour
{
    [SerializeField] private GameObject player;

    public void Start()
    {
        RelativePositionControl.Instance.StartControl(player, Quaternion.identity, Quaternion.identity);
        RelativePositionControl.Instance.ChangeControl(LocomotionControl.Swim);
    }
}