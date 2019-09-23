using System;
using UnityEngine;

public class AndroidPermissionCallback : AndroidJavaProxy
{
    private event Action<string> OnPermissionGrantedAction;
    private event Action<string> OnPermissionDeniedAction;
    private event Action<string> OnPermissionDeniedAndDontAskAgainAction;

    public AndroidPermissionCallback(Action<string> onGrantedCallback, Action<string> onDeniedCallback, Action<string> onDeniedAndDontAskAgainCallback)
        : base("com.ToolBar.EasyWebCam.UnityAndroidPermissions$IPermissionRequestResult2")
    {
        if (onGrantedCallback != null)
        {
            OnPermissionGrantedAction += onGrantedCallback;
        }
        if (onDeniedCallback != null)
        {
            OnPermissionDeniedAction += onDeniedCallback;
        }
        if (onDeniedAndDontAskAgainCallback != null)
        {
            OnPermissionDeniedAndDontAskAgainAction += onDeniedAndDontAskAgainCallback;
        }
    }

    // Handle permission granted
    public virtual void OnPermissionGranted(string permissionName)
    {
        //Debug.Log("Permission " + permissionName + " GRANTED");
        if (OnPermissionGrantedAction != null)
        {
            OnPermissionGrantedAction(permissionName);
        }
    }

    // Handle permission denied
    public virtual void OnPermissionDenied(string permissionName)
    {
        //Debug.Log("Permission " + permissionName + " DENIED!");
        if (OnPermissionDeniedAction != null)
        {
            OnPermissionDeniedAction(permissionName);
        }
    }

    // Handle permission denied and 'Dont ask again' selected
    // Note: falls back to OnPermissionDenied() if action not registered
    public virtual void OnPermissionDeniedAndDontAskAgain(string permissionName)
    {
        //Debug.Log("Permission " + permissionName + " DENIED and 'Dont ask again' was selected!");
        if (OnPermissionDeniedAndDontAskAgainAction != null)
        {
            OnPermissionDeniedAndDontAskAgainAction(permissionName);
        }
        else if (OnPermissionDeniedAction != null)
        {
            // Fall back to OnPermissionDeniedAction
            OnPermissionDeniedAction(permissionName);
        }
    }
}

public class CameraPermissionsController
{
    private static AndroidJavaObject m_Activity;
    private static AndroidJavaObject m_PermissionService;

    private static AndroidJavaObject GetActivity()
    {
        if (m_Activity == null)
        {
            var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            m_Activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        }
        return m_Activity;
    }

    private static AndroidJavaObject GetPermissionsService()
    {
        return m_PermissionService ??
            (m_PermissionService = new AndroidJavaObject("com.ToolBar.EasyWebCam.UnityAndroidPermissions"));
    }

    public static bool IsPermissionGranted(string permissionName)
    {
        return GetPermissionsService().Call<bool>("IsPermissionGranted", GetActivity(), permissionName);
    }

    public static void RequestPermission(string permissionName, AndroidPermissionCallback callback)
    {
        RequestPermission(new[] {permissionName}, callback);
    }

    public static void RequestPermission(string[] permissionNames, AndroidPermissionCallback callback)
    {
        GetPermissionsService().Call("RequestPermissionAsync", GetActivity(), permissionNames, callback);
    }
}
