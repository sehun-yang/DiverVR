#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

#if (UNITY_IOS || UNITY_VISIONOS) && !UNITY_EDITOR
using System.Collections;
using UnityEngine.iOS;
#endif

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class OpenXRFoveation : MonoBehaviour
{
#if (UNITY_ANDROID || UNITY_IOS || UNITY_VISIONOS) && !UNITY_EDITOR
    private const string eyeTrackingPermission = "com.oculus.permission.EYE_TRACKING";
#endif
    private readonly List<XRDisplaySubsystem> xrDisplays = new ();

    private void ActiveFoveation()
    {
        SubsystemManager.GetSubsystems(xrDisplays);
        if (xrDisplays.Count == 1)
        {
            xrDisplays[0].foveatedRenderingLevel = 1.0f;
            xrDisplays[0].foveatedRenderingFlags = XRDisplaySubsystem.FoveatedRenderingFlags.GazeAllowed;
        }
    }

#if (UNITY_ANDROID || (UNITY_IOS || UNITY_VISIONOS)) && !UNITY_EDITOR
    private bool isRequesting;
#endif
    private bool hasPermission;

    public bool HasPermission
    {
        get
        {
            return hasPermission;
        }
        private set
        {
            Debug.Log($"eye tracking Permission Granted: {value}");
            if (hasPermission != value)
            {
                hasPermission = value;
                if (hasPermission)
                {
                    ActiveFoveation();
                }
            }
        }
    }

    private void Start()
    {
        Initialize();
    }

#if (UNITY_IOS || UNITY_VISIONOS) && !UNITY_EDITOR
        IEnumerator PermissionCheck()
        {
            this.isRequesting = true;
            yield return Application.RequestUserAuthorization(eyeTrackingPermission);
            this.isRequesting = false;
            if (Application.HasUserAuthorization(eyeTrackingPermission))
            {
                this.HasPermission = true;
            }
            else
            {
                this.HasPermission = false;
            }
        }
#endif

    public void Initialize()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (Permission.HasUserAuthorizedPermission(eyeTrackingPermission))
            {
                this.HasPermission = true;
            }
            else
            {
#if UNITY_2020_2_OR_NEWER
                var callbacks = new PermissionCallbacks();
                callbacks.PermissionDenied += PermissionCallbacks_PermissionDenied;
                callbacks.PermissionGranted += PermissionCallbacks_PermissionGranted;
                Permission.RequestUserPermission(eyeTrackingPermission, callbacks);
#else
                Permission.RequestUserPermission(eyeTrackingPermission);
#endif
                this.isRequesting = true;
            }
#elif (UNITY_IOS || UNITY_VISIONOS) && !UNITY_EDITOR
            this.StartCoroutine(this.PermissionCheck());
#else
        HasPermission = true;
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
#if UNITY_2020_2_OR_NEWER
        internal void PermissionCallbacks_PermissionDeniedAndDontAskAgain(string permissionName)
        {
            this.isRequesting = false;
            this.HasPermission = false;
            Debug.Log($"{permissionName} PermissionDeniedAndDontAskAgain");
        }

        internal void PermissionCallbacks_PermissionGranted(string permissionName)
        {
            this.isRequesting = false;
            this.HasPermission = true;
            Debug.Log($"{permissionName} PermissionGranted");
        }

        internal void PermissionCallbacks_PermissionDenied(string permissionName)
        {
            this.isRequesting = false;
            this.HasPermission = false;
            Debug.Log($"{permissionName} PermissionDenied");
        }
#else
        private void OnApplicationFocus(bool focus)
        {
            if (focus && this.isRequesting)
            {
                if (Permission.HasUserAuthorizedPermission(eyeTrackingPermission))
                {
                    this.HasPermission = true;
                }
                else
                {
                    this.HasPermission = false;
                }
                this.isRequesting = false;
            }
        }
#endif
#endif
}