using UnityEngine;
using UnityEngine.Animations.Rigging;
using Fusion;
using UnityEngine.InputSystem.XR;
using System.Collections.Generic;
using UnityEngine.Audio;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkObject))]
public class PlayerControl : NetworkBehaviour
{
    [Networked] public Vector3 CharacterPosition {get; set;}
    [Networked] public HalfQuaternion CharacterRotation {get; set;}

    [Networked] public Vector3 LHPosition {get; set;}
    [Networked] public HalfQuaternion LHRotation {get; set;}

    [Networked] public Vector3 RHPosition {get; set;}
    [Networked] public HalfQuaternion RHRotation { get; set; }

    [Networked] public NetworkBool LeftHandIndex { get; set; }
    [Networked] public NetworkBool LeftHandPinky { get; set; }
    [Networked] public NetworkBool RightHandIndex { get; set; }
    [Networked] public NetworkBool RightHandPinky { get; set; }

    [SerializeField] private RigBuilder _rigBuilder;
    [SerializeField] private Rigidbody _bodyRigidbody;
    [SerializeField] private TwoBoneIKConstraint _leftIK;
    [SerializeField] private TwoBoneIKConstraint _rightIK;
    [SerializeField] private TrackedPoseDriver _poseDriver;
    [SerializeField] private List<Renderer> renderers;
    [SerializeField] private CapsuleCollider _bodyCollider;
    [SerializeField] private Collider _leftFingerCollider;
    [SerializeField] private Collider _rightFingerCollider;
    [SerializeField] private Collider _leftHandCollider;
    [SerializeField] private Collider _rightHandCollider;
    [SerializeField] private GameObject[] _transparentToMe;
    [SerializeField] private Transform _tipLeft;
    [SerializeField] private Transform _tipRight;
    [SerializeField] private Transform _leftFingerPointer;
    [SerializeField] private Transform _rightFingerPointer;
    [SerializeField] private AudioSource _micAudioSource;
    [SerializeField] private AudioMixerGroup _loopbackgroup;
    [SerializeField] private Transform _mouthBone;
    [SerializeField] private Vector3 originalMouthAngles;
    [SerializeField] private float _mouthBoneRotationMin;
    [SerializeField] private float _mouthBoneRotationMax;
    
    [SerializeField] private Transform _leftHandIndexBone;
    [SerializeField] private Transform _leftHandIndexBone2;
    [SerializeField] private Transform _leftHandPinkyBone;
    [SerializeField] private Transform _leftHandPinkyBone2;
    [SerializeField] private Transform _leftHandThumbBone;
    [SerializeField] private Transform _leftHandThumbBone2;
    [SerializeField] private Transform _rightHandIndexBone;
    [SerializeField] private Transform _rightHandIndexBone2;
    [SerializeField] private Transform _rightHandPinkyBone;
    [SerializeField] private Transform _rightHandPinkyBone2;
    [SerializeField] private Transform _rightHandThumbBone;
    [SerializeField] private Transform _rightHandThumbBone2;

    private readonly Quaternion FingerFoldRotation = Quaternion.Euler(0, 0, 90);
    private readonly Quaternion FingerUnfoldRotation = Quaternion.Euler(0, 0, 0);
    
    private readonly Quaternion ThumbFingerFoldRotation = Quaternion.Euler(90, 0, 0);
    private readonly Quaternion ThumbFingerUnfoldRotation = Quaternion.Euler(0, 0, 0);

    public Vector3 HeadOriginOffset = new(0, 0, -0.15f);
    public Vector3 EyeOriginOffset = new (0, 0, 0.064f);
    public Vector3 BodyCenterOffset = new (0, -0.12f, -0.22f);

    public Vector3 ArmLeftOriginOffset;
    public Vector3 ArmRightOriginOffset;

    private GameObject leftHandTransform;
    private GameObject rightHandTransform;
    private float lastMicVolume = 0;

    public Rigidbody BodyRigidbody => _bodyRigidbody;
    public CapsuleCollider BodyCollider => _bodyCollider;
    public RigBuilder RigBuilder => _rigBuilder;
    public TwoBoneIKConstraint LeftIK => _leftIK;
    public TwoBoneIKConstraint RightIK => _rightIK;
    public Vector3 RightHandPosition => _rightHandCollider.transform.position;

    public readonly Dictionary<ControllerButtonType, bool> ButtonPressState = new ()
    {
        { ControllerButtonType.LeftTrigger, false},
        { ControllerButtonType.LeftGrip, false},
        { ControllerButtonType.RightGrip, false},
        { ControllerButtonType.RightTrigger, false}
    };

    private void OnButtonPerformed(ControllerButtonType type, bool on)
    {
        ButtonPressState[type] = on;
    }

    public Ray GetHandsRay(HandsDirection handsDirection)
    {
        if (handsDirection == HandsDirection.Left)
        {
            return new Ray(_leftFingerPointer.position, _leftFingerPointer.right);
        }
        else if (handsDirection == HandsDirection.Right)
        {
            return new Ray(_rightFingerPointer.position, _rightFingerPointer.right);
        }

        return new Ray();
    }

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            var inverseRotation = Quaternion.Inverse(transform.rotation);
            RelativePositionControl.Instance.StartControl(gameObject, inverseRotation * _tipLeft.rotation, inverseRotation * _tipRight.rotation);
            _poseDriver.enabled = true;

            _leftHandCollider.enabled = true;
            _rightHandCollider.enabled = true;
            _leftFingerCollider.enabled = true;
            _rightFingerCollider.enabled = true;
            _bodyCollider.enabled = true;

            int transparentLayer = LayerMask.NameToLayer("TransparentToMe");
            foreach (var transparentPart in _transparentToMe)
            {
                transparentPart.layer = transparentLayer;
            }
            foreach (var renderer in renderers)
            {
                (renderer as SkinnedMeshRenderer).updateWhenOffscreen = true;
            }
            _micAudioSource.outputAudioMixerGroup = _loopbackgroup;
        }
        else
        {
            leftHandTransform = new GameObject("_LH");
            rightHandTransform = new GameObject("_RH");

            _bodyRigidbody.isKinematic = true;

            _leftIK.data.target = leftHandTransform.transform;
            _rightIK.data.target = rightHandTransform.transform;

            RigBuilder.Build();
        }
    }

    private void OnDestroy()
    {
        if (leftHandTransform != null)
        {
            Destroy(leftHandTransform);
        }
        if (rightHandTransform != null)
        {
            Destroy(rightHandTransform);
        }
    }

    private void Interpolate(Transform targetTransform, Vector3 goalPosition, Quaternion goalRotation)
    {
        float alpha = 0.6f;
        targetTransform.SetPositionAndRotation(Vector3.Lerp(targetTransform.position, goalPosition, alpha), Quaternion.Slerp(targetTransform.rotation, goalRotation, alpha));
    }

    private void InterpolateLocal(Transform targetTransform, Vector3 goalPosition, Quaternion goalRotation)
    {
        float alpha = 0.6f;
        targetTransform.SetLocalPositionAndRotation(Vector3.Lerp(targetTransform.localPosition, goalPosition, alpha), Quaternion.Slerp(targetTransform.localRotation, goalRotation, alpha));
    }

    private void InterpolateEverything()
    {
        UpdateFinger();
        Interpolate(transform, CharacterPosition, CharacterRotation);
        if (leftHandTransform != null)
        {
            Interpolate(leftHandTransform.transform, LHPosition, LHRotation);
        }
        if (rightHandTransform != null)
        {
            Interpolate(rightHandTransform.transform, RHPosition, RHRotation);
        }
    }

    private void UpdateFinger()
    {
        InterpolateLocal(_leftHandIndexBone, _leftHandIndexBone.localPosition, LeftHandIndex ? FingerFoldRotation : FingerUnfoldRotation);
        InterpolateLocal(_leftHandIndexBone2, _leftHandIndexBone2.localPosition, LeftHandIndex ? FingerFoldRotation : FingerUnfoldRotation);
        InterpolateLocal(_leftHandPinkyBone, _leftHandPinkyBone.localPosition, LeftHandPinky ? FingerFoldRotation : FingerUnfoldRotation);
        InterpolateLocal(_leftHandPinkyBone2, _leftHandPinkyBone2.localPosition, LeftHandPinky ? FingerFoldRotation : FingerUnfoldRotation);
        InterpolateLocal(_rightHandIndexBone, _rightHandIndexBone.localPosition, RightHandIndex ? FingerFoldRotation : FingerUnfoldRotation);
        InterpolateLocal(_rightHandIndexBone2, _rightHandIndexBone2.localPosition, RightHandIndex ? FingerFoldRotation : FingerUnfoldRotation);
        InterpolateLocal(_rightHandPinkyBone, _rightHandPinkyBone.localPosition, RightHandPinky ? FingerFoldRotation : FingerUnfoldRotation);
        InterpolateLocal(_rightHandPinkyBone2, _rightHandPinkyBone2.localPosition, RightHandPinky ? FingerFoldRotation : FingerUnfoldRotation);
        
        InterpolateLocal(_leftHandThumbBone, _leftHandThumbBone.localPosition, (LeftHandPinky && LeftHandIndex) ? ThumbFingerFoldRotation : ThumbFingerUnfoldRotation);
        InterpolateLocal(_leftHandThumbBone2, _leftHandThumbBone2.localPosition, (LeftHandPinky && LeftHandIndex) ? ThumbFingerFoldRotation : ThumbFingerUnfoldRotation);
        InterpolateLocal(_rightHandThumbBone, _rightHandThumbBone.localPosition, (RightHandPinky && RightHandIndex) ? ThumbFingerFoldRotation : ThumbFingerUnfoldRotation);
        InterpolateLocal(_rightHandThumbBone2, _rightHandThumbBone2.localPosition, (RightHandPinky && RightHandIndex) ? ThumbFingerFoldRotation : ThumbFingerUnfoldRotation);
    }

    private void UpdateMyFinger()
    {
        InterpolateLocal(_leftHandIndexBone, _leftHandIndexBone.localPosition, ButtonPressState[ControllerButtonType.LeftTrigger] ? FingerFoldRotation : FingerUnfoldRotation);
        InterpolateLocal(_leftHandIndexBone2, _leftHandIndexBone2.localPosition, ButtonPressState[ControllerButtonType.LeftTrigger] ? FingerFoldRotation : FingerUnfoldRotation);
        InterpolateLocal(_leftHandPinkyBone, _leftHandPinkyBone.localPosition, ButtonPressState[ControllerButtonType.LeftGrip] ? FingerFoldRotation : FingerUnfoldRotation);
        InterpolateLocal(_leftHandPinkyBone2, _leftHandPinkyBone2.localPosition, ButtonPressState[ControllerButtonType.LeftGrip] ? FingerFoldRotation : FingerUnfoldRotation);
        InterpolateLocal(_rightHandIndexBone, _rightHandIndexBone.localPosition, ButtonPressState[ControllerButtonType.RightTrigger] ? FingerFoldRotation : FingerUnfoldRotation);
        InterpolateLocal(_rightHandIndexBone2, _rightHandIndexBone2.localPosition, ButtonPressState[ControllerButtonType.RightTrigger] ? FingerFoldRotation : FingerUnfoldRotation);
        InterpolateLocal(_rightHandPinkyBone, _rightHandPinkyBone.localPosition, ButtonPressState[ControllerButtonType.RightGrip] ? FingerFoldRotation : FingerUnfoldRotation);
        InterpolateLocal(_rightHandPinkyBone2, _rightHandPinkyBone2.localPosition, ButtonPressState[ControllerButtonType.RightGrip] ? FingerFoldRotation : FingerUnfoldRotation);
        
        InterpolateLocal(_leftHandThumbBone, _leftHandThumbBone.localPosition, (ButtonPressState[ControllerButtonType.LeftTrigger] && ButtonPressState[ControllerButtonType.LeftGrip]) ? ThumbFingerFoldRotation : ThumbFingerUnfoldRotation);
        InterpolateLocal(_leftHandThumbBone2, _leftHandThumbBone2.localPosition, (ButtonPressState[ControllerButtonType.LeftTrigger] && ButtonPressState[ControllerButtonType.LeftGrip]) ? ThumbFingerFoldRotation : ThumbFingerUnfoldRotation);
        InterpolateLocal(_rightHandThumbBone, _rightHandThumbBone.localPosition, (ButtonPressState[ControllerButtonType.RightTrigger] && ButtonPressState[ControllerButtonType.RightGrip]) ? ThumbFingerFoldRotation : ThumbFingerUnfoldRotation);
        InterpolateLocal(_rightHandThumbBone2, _rightHandThumbBone2.localPosition, (ButtonPressState[ControllerButtonType.RightTrigger] && ButtonPressState[ControllerButtonType.RightGrip]) ? ThumbFingerFoldRotation : ThumbFingerUnfoldRotation);
    }

    public void FixedUpdate()
    {
        if (HasStateAuthority)
        {
            UpdateMyFinger();
        }
        else
        {
            InterpolateEverything();
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority)
        {
            CharacterPosition = transform.position;
            CharacterRotation = transform.rotation;

            LHPosition = _leftIK.data.target.position;
            LHRotation = _leftIK.data.target.rotation;

            RHPosition = _rightIK.data.target.position;
            RHRotation = _rightIK.data.target.rotation;

            LeftHandIndex = ButtonPressState[ControllerButtonType.LeftTrigger];
            LeftHandPinky = ButtonPressState[ControllerButtonType.LeftGrip];
            RightHandIndex = ButtonPressState[ControllerButtonType.RightTrigger];
            RightHandPinky = ButtonPressState[ControllerButtonType.RightGrip];
        }
        else
        {
            InterpolateEverything();
        }
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        float sum = 0f;
        for (int i = 0; i < data.Length; i++)
        {
            sum += data[i] * data[i];
        }
        var rmsValue = Mathf.Sqrt(sum / data.Length);
        lastMicVolume = 20f * Mathf.Log10(rmsValue / 0.1f);
    }

    private void UpdateMouth()
    {
        if (_mouthBone == null) return;

        float alpha = Mathf.Clamp((lastMicVolume + 50) / 30, 0, 1);
#if UNITY_EDITOR
        alpha = Mathf.Clamp(Mathf.Sin(Time.realtimeSinceStartup * 2), 0, 1);
#endif
        if (alpha > 0)
        {
            _mouthBone.localRotation = Quaternion.Euler(_mouthBoneRotationMin + alpha * (_mouthBoneRotationMax - _mouthBoneRotationMin), originalMouthAngles.y, originalMouthAngles.z);
        }
        else
        {
            _mouthBone.localRotation = Quaternion.Euler(originalMouthAngles.x, originalMouthAngles.y, originalMouthAngles.z);
        }
    }

    private void Update()
    {
        UpdateMouth();
    }
}