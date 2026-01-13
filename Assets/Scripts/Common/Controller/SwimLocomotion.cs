using UnityEngine;
using UnityEngine.Animations.Rigging;

public partial class RelativePositionControl
{
    [SerializeField] private Vector3 _underWaterBuoyancy = new (0, 3.0f, 0);
    [SerializeField] private float _swimShootPowerMultiplier = 0.1f;
    [SerializeField] private float _swimShootPowerExponent = 0.15f;
    [SerializeField] private float _swimShootSpeed = 0.35f;

    private void SwimFixedUpdate()
    {
        // buoyancy
        characterRigidbody.AddForce(_underWaterBuoyancy, ForceMode.Acceleration);

        TogglePhysics(true);
    }

    private void SwimUpdate()
    {
        foreach (var hand in _hands)
        {
            HandsDirection handsDirection = hand.Key;
            Transform targetTransform = hand.Value;
            Transform referenceTransform = _referenceMap[handsDirection];

            Vector3 targetPosition = ClampArm(handsDirection, character.position + targetTransform.position - _origin.position);
            Vector3 newRefPos = targetPosition;
            Vector3 currentPos = referenceTransform.position;
            
            Vector3 lastPosition = lastRelPositions[handsDirection].Position;
            Vector3 ray = targetPosition - lastPosition;

            Vector3 handsSpeed = GetVelocity(handsDirection, targetTransform.position, tick.ElapsedMilliseconds);

            // Disable moving backward
            Vector3 lookForward = RigControl.Instance.transform.forward;
            Vector3 speedForwardDirection = Vector3.Project(handsSpeed, lookForward);
            if (Vector3.Dot(speedForwardDirection, lookForward) > 0)
            {
                handsSpeed -= speedForwardDirection;
            }

            float handsSpeedMag = handsSpeed.magnitude;

            if (handsSpeedMag > _swimShootSpeed)
            {
                float viscocityMultiplier = Mathf.Pow(handsSpeed.sqrMagnitude, _swimShootPowerExponent);
                Shoot(handsDirection, _swimShootPowerMultiplier * viscocityMultiplier * -handsSpeed);
            }
            else
            {
                if (Physics.OverlapSphereNonAlloc(currentPos, 0.00001f, collisionResult, _obstacleLayers) == 0)
                {
                    if (Physics.Raycast(lastPosition, ray.normalized, out RaycastHit hit, Mathf.Max(0.00005f, ray.magnitude), _obstacleLayers))
                    {
                        Shoot(handsDirection, hit.normal, -handsSpeed, false);
                        newRefPos = hit.point + 0.01f * hit.normal;
                    }
                }
            }

            referenceTransform.SetPositionAndRotation(newRefPos, targetTransform.rotation * initialHandsRotations[handsDirection]);
        }
    }
}