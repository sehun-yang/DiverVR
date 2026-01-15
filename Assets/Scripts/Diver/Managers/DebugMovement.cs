using UnityEngine;
using UnityEngine.InputSystem;
using Fusion;
using System.Collections.Generic;

public class DebugMovement : MonoBehaviour
{
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _rotateSpeed = 20f;

    [SerializeField] private Transform _leftHandDevice;
    [SerializeField] private Transform _rightHandDevice;

    [SerializeField] private SerializableDictionary<InputActionReference, Vector3> _referenceMap = new();
    [SerializeField] private SerializableDictionary<InputActionReference, float> _rotationBindings = new();
    [SerializeField] private SerializableDictionary<InputActionReference, Vector3> _leftHandMovement = new();
    [SerializeField] private SerializableDictionary<InputActionReference, Vector3> _rightHandMovement = new();

#if UNITY_EDITOR
    private readonly Dictionary<InputAction, Vector3> movementMap = new();
    private readonly Dictionary<InputAction, float> rotationMap = new();
    private readonly Dictionary<InputAction, Vector3> leftHandsMovmentMap = new();
    private readonly Dictionary<InputAction, Vector3> rightHandsMovmentMap = new();
    private readonly HashSet<InputAction> pressedMap = new();

    private void OnEnable()
    {
        foreach (var pair in _referenceMap)
        {
            pair.Key.action.performed += OnPerformed;
            pair.Key.action.canceled += OnCanceled;
            movementMap.Add(pair.Key.action, pair.Value);
        }
        foreach (var pair in _rotationBindings)
        {
            pair.Key.action.performed += OnPerformed;
            pair.Key.action.canceled += OnCanceled;
            rotationMap.Add(pair.Key.action, pair.Value);
        }
        foreach (var pair in _leftHandMovement)
        {
            pair.Key.action.performed += OnPerformed;
            pair.Key.action.canceled += OnCanceled;
            leftHandsMovmentMap.Add(pair.Key.action, pair.Value);
        }
        foreach (var pair in _rightHandMovement)
        {
            pair.Key.action.performed += OnPerformed;
            pair.Key.action.canceled += OnCanceled;
            rightHandsMovmentMap.Add(pair.Key.action, pair.Value);
        }
    }

    private void OnPerformed(InputAction.CallbackContext context)
    {
        pressedMap.Add(context.action);
    }

    private void OnCanceled(InputAction.CallbackContext context)
    {
        pressedMap.Remove(context.action);
    }

    private void Update()
    {
        var myCharacter = RelativePositionControl.Instance.MyPlayerControl;
        if (myCharacter != null)
        {
            Camera mainCamera = Camera.main;
            Vector3 totalDirection = Vector3.zero;
            float totalRotation = 0f;

            foreach (InputAction action in pressedMap)
            {
                if (movementMap.TryGetValue(action, out Vector3 moveDir))
                {
                    totalDirection += mainCamera.transform.TransformDirection(moveDir);
                }

                if (rotationMap.TryGetValue(action, out float rotationDir))
                {
                    totalRotation += rotationDir;
                }

                if (leftHandsMovmentMap.TryGetValue(action, out Vector3 leftMove))
                {
                    _leftHandDevice.position += myCharacter.transform.TransformVector(leftMove) * Time.deltaTime;
                }
                
                if (rightHandsMovmentMap.TryGetValue(action, out Vector3 rightMove))
                {
                    _rightHandDevice.position += myCharacter.transform.TransformVector(rightMove) * Time.deltaTime;
                }
            }

            myCharacter.transform.position += _speed * Time.deltaTime * totalDirection;
            Camera.main.transform.Rotate(Vector3.up, totalRotation * _rotateSpeed * Time.deltaTime, Space.Self);
        }
    }
#endif
}