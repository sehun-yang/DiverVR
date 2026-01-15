using System.Collections.Generic;
using UnityEngine;

public partial class RelativePositionControl
{
    private readonly Dictionary<HandsDirection, Vector3> nextHandPositionMap = new()
    {
        {HandsDirection.Left, Vector3.zero},
        {HandsDirection.Right, Vector3.zero},
    };

    private void DefaultFixedUpdate()
    {
        if (!characterRigidbody.isKinematic)
        {
            characterRigidbody.angularVelocity = Vector3.zero;
        }
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
        foreach (var hand in _hands)
        {
            HandsDirection handsDirection = hand.Key;
            Transform targetTransform = hand.Value;
            Transform referenceTransform = _referenceMap[handsDirection];

            Vector3 targetPosition = ClampArm(handsDirection, character.position + targetTransform.position - _origin.position);
            Vector3 currentPos = referenceTransform.position;
            Vector3 newRefPos = targetPosition;

            Vector3 lastPosition = lastRelPositions[handsDirection].Position;
            Vector3 ray = targetPosition - lastPosition;
            Vector3 handsSpeed = GetVelocity(handsDirection, targetTransform.position, tick.ElapsedMilliseconds);
            float handsSpeedMag = handsSpeed.magnitude;

            bool isShotRetained = shotExpireTimer[handsDirection].ExpireAt > tick.ElapsedMilliseconds;

            if (isShotRetained)
            {
                Shoot(handsDirection, shotExpireTimer[handsDirection].Normal, -handsSpeed, false);
            }

            if (Physics.Raycast(lastPosition, ray.normalized, out RaycastHit hit, ray.magnitude, _obstacleLayers))
            {
                bool hasTerrainOverride = hit.transform.gameObject.TryGetComponent(out TerrainPropertyOverride terrain);

                float maxClimbAngle = hasTerrainOverride ? terrain.MaxClimbAngle : _normalTerrainMaxClimbAngle;
                if (!isShotRetained)
                {
                    //EventBus.RaiseHapticFeedback(handsDirection, _contactImpulseAmplitude, _contactImpulseDuration);
                }

                totalElevation += ApplyRepulsivePower(targetPosition, hit.point, hit.normal);

                if (!isShotRetained)
                {
                    Shoot(handsDirection, hit.normal, -handsSpeed, true);
                }
            }

            nextHandPositionMap[handsDirection] = newRefPos;
        }

        characterRigidbody.MovePosition(character.position + totalElevation);

        foreach (var hand in _hands)
        {
            HandsDirection handsDirection = hand.Key;
            Transform referenceTransform = _referenceMap[handsDirection];
            Transform targetTransform = hand.Value;

            referenceTransform.SetPositionAndRotation(nextHandPositionMap[handsDirection] + totalElevation, targetTransform.rotation * initialHandsRotations[handsDirection]);
        }

        RigControl.Instance.UpdateWindParticle(characterRigidbody.linearVelocity);
    }
}