using System.Collections.Generic;
using UnityEngine;

public partial class RelativePositionControl
{
    private readonly Dictionary<HandsDirection, Vector3> nextHandPositionMap = new()
    {
        {HandsDirection.Left, Vector3.zero},
        {HandsDirection.Right, Vector3.zero},
    };

    private readonly Dictionary<HandsDirection, bool> isHandColliding = new()
    {
        {HandsDirection.Left, false},
        {HandsDirection.Right, false},
    };
    private readonly Dictionary<HandsDirection, Vector3> lastHitNormal = new()
    {
        {HandsDirection.Left, Vector3.zero},
        {HandsDirection.Right, Vector3.zero},
    };

    private void DefaultFixedUpdate()
    {
    }

    private Vector3 ApplyRepulsivePower(Vector3 targetPosition, Vector3 hitPoint, Vector3 hitNormal)
    {
        Vector3 contactPoint = hitPoint + hitNormal * 0.001f;
        Vector3 overlapVector = contactPoint - targetPosition;

        return overlapVector;
    }

    private void DefaultUpdate()
    {
        Vector3 totalElevation = Vector3.zero;
        bool doElevation = false;

        foreach (var hand in _hands)
        {
            HandsDirection handsDirection = hand.Key;
            Transform targetTransform = hand.Value;

            Vector3 targetPosition = ClampArm(handsDirection, character.position + targetTransform.position - _origin.position);
            Vector3 lastPosition = lastRelPositions[handsDirection].Position;
            Vector3 ray = targetPosition - lastPosition;
            Vector3 handsSpeed = GetVelocity(handsDirection, targetTransform.position, tick.ElapsedMilliseconds);

            bool isShotRetained = shotExpireTimer[handsDirection].ExpireAt > tick.ElapsedMilliseconds;

            if (isShotRetained)
            {
                Shoot(handsDirection, shotExpireTimer[handsDirection].Normal, -handsSpeed, false);
            }

            if (Physics.Raycast(lastPosition, ray.normalized, out RaycastHit hit, ray.magnitude, _obstacleLayers))
            {
                isHandColliding[handsDirection] = true;
                lastHitNormal[handsDirection] = hit.normal;

                doElevation = true;
                Vector3 elevation = ApplyRepulsivePower(targetPosition, hit.point, hit.normal);

                if (elevation.sqrMagnitude > totalElevation.sqrMagnitude)
                {
                    totalElevation = elevation;
                }

                if (!isShotRetained)
                {
                    Shoot(handsDirection, hit.normal, -handsSpeed, true);
                }
            }
            else if (isHandColliding[handsDirection])
            {
                Vector3 normal = lastHitNormal[handsDirection];
                
                float pushing = Vector3.Dot(ray, -normal);
                
                if (pushing > 0.001f)
                {
                    doElevation = true;
                    Vector3 elevation = normal * pushing;

                    if (elevation.sqrMagnitude > totalElevation.sqrMagnitude)
                    {
                        totalElevation = elevation;
                    }
                }
                else
                {
                    isHandColliding[handsDirection] = false;
                }
            }

            nextHandPositionMap[handsDirection] = targetPosition;
        }

        if (doElevation)
        {
            const float maxElevation = 0.3f;
            if (totalElevation.magnitude > maxElevation)
            {
                totalElevation = totalElevation.normalized * maxElevation;
            }

            character.position += totalElevation;
        }

        foreach (var hand in _hands)
        {
            HandsDirection handsDirection = hand.Key;
            Transform referenceTransform = _referenceMap[handsDirection];
            Transform targetTransform = hand.Value;

            referenceTransform.SetPositionAndRotation(
                nextHandPositionMap[handsDirection] + totalElevation,
                targetTransform.rotation * initialHandsRotations[handsDirection]
            );
        }

        RigControl.Instance.UpdateWindParticle(characterRigidbody.linearVelocity);
    }
}