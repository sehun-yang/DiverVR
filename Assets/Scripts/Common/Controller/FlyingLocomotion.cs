using UnityEngine;

public partial class RelativePositionControl
{
    [SerializeField] private float _flyingShootPowerMultiplier = 0.1f;
    [SerializeField] private float _flyingShootPowerExponent = 0.15f;
    [SerializeField] private float _flyingShootSpeed = 0.35f;

    private void FlyingFixedUpdate()
    {
    }

    private void FlyingUpdate()
    {
        foreach (var hand in _hands)
        {
            HandsDirection handsDirection = hand.Key;
            Transform targetTransform = hand.Value;
            Transform referenceTransform = _referenceMap[handsDirection];

            Vector3 targetPosition = ClampArm(handsDirection, character.position + targetTransform.position - _origin.position);
            Vector3 newRefPos = targetPosition;
            
            Vector3 handsSpeed = GetVelocity(handsDirection, targetTransform.position, tick.ElapsedMilliseconds);

            // Disable moving backward
            Vector3 lookForward = RigControl.Instance.transform.forward;
            Vector3 speedForwardDirection = Vector3.Project(handsSpeed, lookForward);
            if (Vector3.Dot(speedForwardDirection, lookForward) > 0)
            {
                handsSpeed -= speedForwardDirection;
            }
            
            // Disable moving downward
            Vector3 speedUpDirection = Vector3.Project(handsSpeed, Vector3.up);
            if (Vector3.Dot(speedUpDirection, Vector3.up) > 0)
            {
                handsSpeed -= speedUpDirection;
            }

            float handsSpeedMag = handsSpeed.magnitude;

            if (handsSpeedMag > _flyingShootSpeed)
            {
                float viscocityMultiplier = Mathf.Pow(handsSpeed.sqrMagnitude, _flyingShootPowerExponent);
                Shoot(handsDirection, _flyingShootPowerMultiplier * viscocityMultiplier * -handsSpeed);
            }

            referenceTransform.SetPositionAndRotation(newRefPos, targetTransform.rotation * initialHandsRotations[handsDirection]);
        }
    }
}