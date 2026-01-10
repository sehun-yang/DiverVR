using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.XR;

public partial class RelativePositionControl : SingletonMonoBehaviour<RelativePositionControl>
{
    private class ContactInfo
    {
        public bool Contacted;
        public long LastContactTime;
        public Vector3 Normal;
        public Vector3 DevicePositionAtContact;
        public Vector3 ContactCharacerPosition;
        public float BreakDistance;
        public float BreakDistanceNormalDirection;
        public float BreakSpeed;

        public bool PlatformCanMove;
        public Transform ContactObjectTransform;
        public Matrix4x4 ContactObjectInverseMatrixOld;

        public Vector3 UpdateVector(Vector3 point)
        {
            return ContactObjectTransform.TransformVector(ContactObjectInverseMatrixOld.MultiplyVector(point));
        }
        public Vector3 UpdatePoint(Vector3 point)
        {
            return ContactObjectTransform.TransformPoint(ContactObjectInverseMatrixOld.MultiplyPoint3x4(point));
        }
        
        public void UpdateContactObject()
        {
            Normal = ContactObjectTransform.TransformVector(ContactObjectInverseMatrixOld.MultiplyVector(Normal));
            ContactCharacerPosition = ContactObjectTransform.TransformPoint(ContactObjectInverseMatrixOld.MultiplyPoint3x4(ContactCharacerPosition));

            ContactObjectInverseMatrixOld = ContactObjectTransform.worldToLocalMatrix;
        }
    }

    private struct PositionInfo
    {
        public Vector3 Position;
        public long Time;
    }

    private class ShotInfo
    {
        public long ExpireAt;
        public Vector3 Normal;
    }

    private const long ShotMinInterval = 0;
    private const float MaxShootVelocity = 1.5f;
    private const int TrackingVelocityCount = 2;

    private LocomotionControl locomotionControl = LocomotionControl.Default;
    public bool started = false;
    private readonly Stopwatch tick = new();
    private readonly Dictionary<HandsDirection, long> shotTimer = new ()
    {
        {HandsDirection.Left, 0},
        {HandsDirection.Right, 0},
    };
    private readonly Dictionary<HandsDirection, ShotInfo> shotExpireTimer = new ()
    {
        {HandsDirection.Left, new ()},
        {HandsDirection.Right, new ()},
    };
    private readonly Collider[] collisionResult = new Collider[1];
    private Transform character;
    private Rigidbody characterRigidbody;
    private CapsuleCollider characterCollider;
    private readonly Dictionary<HandsDirection, Vector3> armOriginOffset = new();

    [SerializeField] private AudioClip _jumpSound;
    [SerializeField] private List<Transform> _deviceTransform;
    private readonly Dictionary<HandsDirection, Transform> _hands = new();
    [SerializeField] private List<Transform> _reference;
    private readonly Dictionary<HandsDirection, Transform> _referenceMap = new();
    private readonly Dictionary<HandsDirection, SphereCollider> _referenceColliderMap = new();
    [SerializeField] private Transform _origin;
    [SerializeField] private LayerMask _obstacleLayers;
    [SerializeField] private float _breakSpeed = 5;
    [SerializeField] private float _shootSpeed = 2;
    [SerializeField] private long _contactCooltime = 150;
    [SerializeField] private float _shootPowerMultiplier = 50f;
    [SerializeField] private float _contactBreakDistance = 0.65f;
    [SerializeField] private float _contactBreakRadiusNormalDirection = 0.05f;
    [SerializeField] private float _normalTerrainMaxClimbAngle = 60f;
    [SerializeField] private float _maxArmLength = 5f;
    [SerializeField] private float _contactImpulseAmplitude = 1;
    [SerializeField] private float _contactImpulseDuration = 0.5f;
    [SerializeField] private long _shotRetainTime = 70;

    private readonly Dictionary<HandsDirection, PositionInfo> lastRelPositions = new() { };
    private readonly Dictionary<HandsDirection, PositionInfo> lastDevPositions = new() { };
    private readonly Dictionary<HandsDirection, ContactInfo> contacted = new()
    {
        {HandsDirection.Left, new ContactInfo() {
            Contacted = false
        }},
        {HandsDirection.Right, new ContactInfo() {
            Contacted = false
        }}
    };
    private readonly Dictionary<HandsDirection, LinkedList<PositionInfo>> velocityTrackers = new()
    {
        {HandsDirection.Left, new () {}},
        {HandsDirection.Right, new () {}},
    };
    private readonly Dictionary<HandsDirection, Quaternion> initialHandsRotations = new()
    {
        {HandsDirection.Left, Quaternion.identity},
        {HandsDirection.Right, Quaternion.identity},
    };

    public void ToggleGorillaTagLocomotion()
    {
        started = !started;
        if (started)
        {
            Initialize();
        }
    }

    private void StorePosition()
    {
        foreach (var kv in _hands)
        {
            var handsDirection = kv.Key;
            lastRelPositions[handsDirection] = new()
            {
                Position = _referenceMap[handsDirection].position,
                Time = tick.ElapsedMilliseconds,
            };

            PositionInfo devicePositionInfo = new()
            {
                Position = _hands[handsDirection].position,
                Time = tick.ElapsedMilliseconds,
            };
            lastDevPositions[handsDirection] = devicePositionInfo;

            velocityTrackers[handsDirection].AddLast(devicePositionInfo);
            if (velocityTrackers[handsDirection].Count > TrackingVelocityCount)
            {
                velocityTrackers[handsDirection].RemoveFirst();
            }
        }
    }
    
    private void Shoot(HandsDirection handsDirection, Vector3 shootVector)
    {
        float originPower = shootVector.magnitude;
        float finalPower = Mathf.Min(_shootPowerMultiplier * originPower, MaxShootVelocity);
        characterRigidbody.AddForce(shootVector.normalized * finalPower, ForceMode.VelocityChange);
    }
    
    private bool Shoot(HandsDirection handsDirection, Vector3 projectNormal, Vector3 direction, bool initialShoot)
    {
        long time = tick.ElapsedMilliseconds;
        if (shotTimer[handsDirection] < time)
        {
            if (initialShoot)
            {
                shotExpireTimer[handsDirection].ExpireAt = tick.ElapsedMilliseconds + _shotRetainTime;
                shotExpireTimer[handsDirection].Normal = projectNormal;
            }
            shotTimer[handsDirection] = time + ShotMinInterval;

            foreach (var contactInfo in contacted)
            {
                contactInfo.Value.Contacted = false;
            }
            TogglePhysics(true);

            if (Vector3.Dot(direction, projectNormal) > 0 && direction.sqrMagnitude > _shootSpeed * _shootSpeed)
            {
                Shoot(handsDirection, direction);
            }

            return true;
        }
        else
        {
            return false;
        }
    }

    private Vector3 GetVelocity(HandsDirection handsDirection, Vector3 currentPosition, float currentTime)
    {
        XRNode targetHand = handsDirection switch
        {
            HandsDirection.Left => XRNode.LeftHand,
            HandsDirection.Right => XRNode.RightHand,
            _ => throw new System.NotImplementedException(),
        };
        InputDevice hand = InputDevices.GetDeviceAtXRNode(targetHand);

        if (hand.TryGetFeatureValue(CommonUsages.deviceVelocity, out Vector3 velocity))
        {
            return velocity;
        }
        else
        {
            // fallback legacy
            var velocityTracker = velocityTrackers[handsDirection];
            if (velocityTracker.Count <= 0) return Vector3.zero;
            var first = velocityTracker.First.Value;

            return (currentPosition - first.Position) / ((currentTime - first.Time) / 1000.0f);
        }
    }

    private HandsDirection GetOtherSide(HandsDirection otherSide)
    {
        if (otherSide == HandsDirection.Left)
        {
            return HandsDirection.Right;
        }
        else
        {
            return HandsDirection.Left;
        }
    }

    private void OnForcePushCharacter(Vector3 velocity)
    {
        foreach (var kv in contacted)
        {
            var contactInfo = kv.Value;
            contactInfo.Contacted = false;
            contactInfo.LastContactTime = tick.ElapsedMilliseconds;
        }

        characterRigidbody.isKinematic = false;
        characterRigidbody.linearVelocity = velocity;
        characterRigidbody.angularVelocity = Vector3.zero;
    }

    public void ChangeControl(LocomotionControl control)
    {
        Initialize();
        locomotionControl = control;
    }

    public void StartControl(GameObject playerRig, Quaternion leftRotationOffset, Quaternion rightRotationOffset)
    {
        character = playerRig.transform;
        characterRigidbody = playerRig.GetComponent<Rigidbody>();
        characterCollider = playerRig.GetComponent<CapsuleCollider>();

        initialHandsRotations[HandsDirection.Left] = leftRotationOffset;
        initialHandsRotations[HandsDirection.Right] = rightRotationOffset;

        armOriginOffset[HandsDirection.Left] = Vector3.zero; // playerControl.ArmLeftOriginOffset;
        armOriginOffset[HandsDirection.Right] = Vector3.zero; // playerControl.ArmRightOriginOffset;

        Initialize();

        started = true;
    }

    private void TogglePhysics(bool on)
    {
        characterRigidbody.useGravity = on;
        characterRigidbody.isKinematic = !on;
    }

    private Vector3 ClampArm(HandsDirection handsDirection, Vector3 targetPosition)
    {
        Vector3 originPoint = character.TransformPoint(armOriginOffset[handsDirection]);
        Vector3 direction = targetPosition - originPoint;
        if (direction.sqrMagnitude > _maxArmLength * _maxArmLength)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.DrawLine(originPoint, originPoint + direction.normalized * _maxArmLength);
#endif
            return originPoint + direction.normalized * _maxArmLength;
        }
        else
        {
#if UNITY_EDITOR
            UnityEngine.Debug.DrawLine(originPoint, targetPosition);
#endif
            return targetPosition;
        }
    }

    private void FixedUpdate()
    {
        if (!started) return;

        switch (locomotionControl)
        {
            case LocomotionControl.Default:
                DefaultFixedUpdate();
                break;
            case LocomotionControl.Swim:
                SwimFixedUpdate();
                break;
            default:
                break;
        }
    }

    private void Update()
    {
        if (!started)
        {
            StorePosition();
            return;
        }

        switch (locomotionControl)
        {
            case LocomotionControl.Default:
                DefaultUpdate();
                break;
            case LocomotionControl.Swim:
                SwimUpdate();
                break;
            default:
                break;
        }

        StorePosition();
    }

    private void Initialize()
    {
        tick.Restart();
        shotTimer[HandsDirection.Left] = 0;
        shotTimer[HandsDirection.Right] = 0;

        foreach (var kv in velocityTrackers)
        {
            kv.Value.Clear();
        }

        foreach (var kv in contacted)
        {
            kv.Value.Contacted = false;
            kv.Value.LastContactTime = 0;
        }

        _hands[HandsDirection.Left] = _deviceTransform[0];
        _hands[HandsDirection.Right] = _deviceTransform[1];

        _referenceMap[HandsDirection.Left] = _reference[0];
        _referenceMap[HandsDirection.Right] = _reference[1];

        _referenceColliderMap[HandsDirection.Left] = _reference[0].GetComponent<SphereCollider>();
        _referenceColliderMap[HandsDirection.Right] = _reference[1].GetComponent<SphereCollider>();

        foreach (var hand in _hands)
        {
            HandsDirection handsDirection = hand.Key;
            Transform targetTransform = hand.Value;

            Vector3 targetPosition = ClampArm(handsDirection, character.position + targetTransform.position - _origin.position);
            _referenceMap[handsDirection].SetPositionAndRotation(targetPosition, targetTransform.rotation * initialHandsRotations[handsDirection]);

            shotExpireTimer[handsDirection].ExpireAt = 0;
        }

        lastRelPositions[HandsDirection.Left] = new PositionInfo()
        {
            Position = _reference[0].position,
            Time = 0
        };
        lastRelPositions[HandsDirection.Right] = new PositionInfo()
        {
            Position = _reference[1].position,
            Time = 0
        };
        lastDevPositions[HandsDirection.Left] = new PositionInfo()
        {
            Position = _deviceTransform[0].position,
            Time = 0
        };
        lastDevPositions[HandsDirection.Right] = new PositionInfo()
        {
            Position = _deviceTransform[1].position,
            Time = 0
        };
    }

    private void PrepareTeleport()
    {
        started = false;
        foreach (var kv in contacted)
        {
            var contactInfo = kv.Value;
            contactInfo.Contacted = false;
        }

        if (!characterRigidbody.isKinematic)
        {
            characterRigidbody.linearVelocity = Vector3.zero;
            characterRigidbody.angularVelocity = Vector3.zero;
        }

        TogglePhysics(false);
    }

    private void EndTeleport()
    {
        Initialize();
        TogglePhysics(true);
        started = true;
    }
}