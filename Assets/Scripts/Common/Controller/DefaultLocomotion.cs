using System.Collections.Generic;
using UnityEngine;

public partial class RelativePositionControl
{
    private readonly Collider[] elevationResults = new Collider[5];
    private readonly Dictionary<HandsDirection, Vector3> nextHandPositionMap = new()
    {
        {HandsDirection.Left, Vector3.zero},
        {HandsDirection.Right, Vector3.zero},
    };

    private Vector3 Elevation()
    {
        Vector3 elevationValue = Vector3.zero;
        PMMath.GetCapsulePoints(characterCollider, out Vector3 point1, out Vector3 point2);
        int hitCount = Physics.OverlapCapsuleNonAlloc(point1, point2, characterCollider.radius * 0.99f, elevationResults, _obstacleLayers);
        if (hitCount > 0)
        {
            float minPenetration = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                Collider col = elevationResults[i];
                if (Physics.ComputePenetration(characterCollider, character.position, character.rotation, col, col.transform.position, col.transform.rotation, out Vector3 direction, out float distance))
                {
                    if (distance < minPenetration)
                    {
                        minPenetration = distance;
                        elevationValue = 1.011f * distance * direction;
                    }
                }
            }
        }

        return elevationValue;
    }

    private void DefaultFixedUpdate()
    {
        bool notContacted = true;
        foreach (var kv in contacted)
        {
            var contactInfo = kv.Value;
            if (contactInfo.Contacted)
            {
                notContacted = false;
                break;
            }
        }

        if (!notContacted)
        {
            if (!characterRigidbody.isKinematic)
            {
                characterRigidbody.linearVelocity = Vector3.zero;
            }
        }
        if (!characterRigidbody.isKinematic)
        {
            characterRigidbody.angularVelocity = Vector3.zero;
        }

        TogglePhysics(notContacted);
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

            var contactInfo = contacted[handsDirection];
            var otherSideContactInfo = contacted[GetOtherSide(handsDirection)];

            bool isShotRetained = shotExpireTimer[handsDirection].ExpireAt > tick.ElapsedMilliseconds;

            if (isShotRetained)
            {
                Shoot(handsDirection, shotExpireTimer[handsDirection].Normal, -handsSpeed, false);
            }

            if (contactInfo.Contacted)
            {
                if (contactInfo.PlatformCanMove)
                {
                    currentPos = contactInfo.UpdatePoint(currentPos);

                    contactInfo.UpdateContactObject();
                }
                Vector3 handsDelta = targetTransform.position - contactInfo.DevicePositionAtContact;

                if (handsSpeedMag > contactInfo.BreakSpeed)
                {
                    if (!isShotRetained)
                    {
                        //EventBus.RaiseHapticFeedback(handsDirection, _contactImpulseAmplitude, _contactImpulseDuration);
                    }

                    if (Vector3.Dot(handsSpeed / handsSpeedMag, contactInfo.Normal) < 0.1f)
                    {
                        Shoot(handsDirection, contactInfo.Normal, -handsSpeed, !isShotRetained);
                    }
                    else
                    {
                        contactInfo.Contacted = false;
                    }
                }
                else if ((contactInfo.DevicePositionAtContact - targetTransform.position).sqrMagnitude > contactInfo.BreakDistance * contactInfo.BreakDistance)
                {
                    contactInfo.Contacted = false;
                }
                else if (Physics.OverlapSphereNonAlloc(currentPos, contactInfo.BreakDistanceNormalDirection, collisionResult, _obstacleLayers) == 0)
                {
                    contactInfo.Contacted = false;
                }
                else
                {
                    Vector3 handsDeltaNormalDirection = Vector3.Project(handsDelta, contactInfo.Normal);
                    if (handsDeltaNormalDirection.sqrMagnitude > _maxArmLength * _maxArmLength * 0.8)
                    {
                        contactInfo.Contacted = false;
                    }
                    else
                    {
                        Vector3 characterDest = contactInfo.ContactCharacerPosition - handsDelta;

                        character.position = characterDest;
                        newRefPos = ClampArm(handsDirection, characterDest + targetTransform.position - _origin.position);

                        // correct ahead calculation
                        if (handsDirection == HandsDirection.Right)
                        {
                            nextHandPositionMap[HandsDirection.Left] = ClampArm(HandsDirection.Left, characterDest + _hands[HandsDirection.Left].position - _origin.position);
                        }
                    }
                }
            }
            else
            {
                if (Physics.Raycast(lastPosition, ray.normalized, out RaycastHit hit, ray.magnitude, _obstacleLayers))
                {
                    bool hasTerrainOverride = hit.transform.gameObject.TryGetComponent(out TerrainPropertyOverride terrain);
                    float escapeSpeed = hasTerrainOverride ? terrain.ShootSpeed : _shootSpeed;

                    float maxClimbAngle = hasTerrainOverride ? terrain.MaxClimbAngle : _normalTerrainMaxClimbAngle;
                    if (!isShotRetained)
                    {
                        //EventBus.RaiseHapticFeedback(handsDirection, _contactImpulseAmplitude, _contactImpulseDuration);
                    }
                    bool canClimb = (Vector3.Dot(hit.normal, Vector3.up) + Mathf.Epsilon) >= Mathf.Cos(maxClimbAngle * Mathf.Deg2Rad);

                    totalElevation += ApplyRepulsivePower(targetPosition, hit.point, hit.normal);

                    if (handsSpeedMag > escapeSpeed)
                    {
                        if (!isShotRetained)
                        {
                            Shoot(handsDirection, hit.normal, -handsSpeed, true);
                        }
                    }
                    else if (canClimb)
                    {
                        if (tick.ElapsedMilliseconds - contactInfo.LastContactTime > _contactCooltime)
                        {
                            shotExpireTimer[handsDirection].ExpireAt = 0;
                            contactInfo.Contacted = true;
                            contactInfo.LastContactTime = tick.ElapsedMilliseconds;
                            contactInfo.Normal = hit.normal;
                            contactInfo.DevicePositionAtContact = targetTransform.position;
                            contactInfo.ContactCharacerPosition = character.position;
                            otherSideContactInfo.Contacted = false;
                            //totalElevation += otherSideContactInfo.Normal * 0.001f; // prevent simultaneous re-contact of another side

                            if (hasTerrainOverride)
                            {
                                contactInfo.BreakDistance = terrain.ContactBreakDistance;
                                contactInfo.BreakSpeed = terrain.ContactBreakSpeed;
                                contactInfo.BreakDistanceNormalDirection = terrain.ContactBreakDistanceNormalDirection;

                                if (terrain.PlatformCanMove)
                                {
                                    contactInfo.ContactObjectTransform = hit.transform;
                                    contactInfo.ContactObjectInverseMatrixOld = hit.transform.worldToLocalMatrix;
                                    contactInfo.PlatformCanMove = true;
                                }
                                else
                                {
                                    contactInfo.PlatformCanMove = false;
                                }
                            }
                            else
                            {
                                contactInfo.BreakDistance = _contactBreakDistance + (1 - hit.normal.y) * 0.01f;
                                contactInfo.BreakSpeed = _breakSpeed;
                                contactInfo.BreakDistanceNormalDirection = _contactBreakRadiusNormalDirection;
                            }
                        }
                    }
                }
                else
                {
                    Vector3 toRefPos = newRefPos - character.position;
                    Vector3 toRefPosNorm = toRefPos.normalized;
                    float toRefDist = toRefPos.magnitude;
                    if (Physics.Raycast(character.position, toRefPosNorm, out RaycastHit hit2, toRefDist + 0.001f, _obstacleLayers))
                    {
                        totalElevation += -(toRefDist - hit2.distance + 0.002f) * toRefPosNorm;
                    }
                }
            }

            nextHandPositionMap[handsDirection] = newRefPos;
        }

        totalElevation += Elevation();

        foreach (var kv in contacted)
        {
            if (kv.Value.Contacted)
            {
                kv.Value.ContactCharacerPosition += totalElevation;
            }
        }

        character.position += totalElevation;

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