using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace CloudSLAM
{
    /// <summary>
    /// A CloudSLAM client dedicated for Unity Cameras.
    /// This component will connect to the CloudSLAM server, and make the 
    /// camera follow the position and orientaiton received form the 
    /// ClouDSLAM server.
    /// A video feed from the device's physical camera will be streamed
    /// to the CloudSLAM server.
    /// For this component to work properly, a CloudSLAM configuration 
    /// must be provided.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CloudSLAMCamera : MonoBehaviour
    {
        public enum TrackingStatus { None, Lost, Tracking }
        
        [Tooltip("Occurs when connection to the CloudSLAM server has been successfully established, and the CloudSLAM camera has been initialized.")]
        public UnityEvent OnOpen;
        
        [Tooltip("Occurs when connection to the CloudSLAM server has been shut down (be either side or means), and the CloudSLAM camera has been deinitialized.")]
        public UnityEvent OnClose;
        
        [Tooltip("Occurs when tracking status of CloudSLAM has changed. (either NONE, TRACKING or LOST). Typically AR content should not be shown when tracking state is not TRACKING.")]
        public CloudSLAMCameraTrackingStatusChangedEvent OnTrackingStatusChanged;
        
        [Tooltip("Occurs when and error has occured in the CloudSLAMCamera. An error will always result in closing the connection to the CloudSLAM server.")]
        public ClouddSLAMCameraErrorEvent OnError;

        [Tooltip("When set a preview from the used physical camera will be added as background to this camera.")]
        public bool ShowVideoPreviewBackground = true;
        [Tooltip("The CloudSLAM client configuration. Typically this is retreived by scanning a configuration QR code, however feel free to input it manually.")]
        public CloudSLAMConfig config;
        [Tooltip("The material used for the background video preview. This should be set to the material provided with the CloudSLAM SDK. The property is here only so that Unity loads it at runtime.")]
        public Material videoPreviewMaterial;

        /// <summary>
        /// A texture that contains a preview from the device's physical camera.
        /// Returns null when not connected to the CloudSLAM server.
        /// </summary>
        /// <value>The video preview texture.</value>
        public Texture2D VideoPreviewTexture { get; private set; }

        /// <summary>
        /// Connects this CloudSLAMCamera to the CloudSLAM server.
        /// Device orientation will be set to ScreenOrientation.LandscapeLeft and locked.
        /// If ShowVideoPreviewBackground was set, a texture with the preview from the
        /// physical camera will be added to this camera as background.
        /// </summary>
        public void StartCloudSLAM()
        {
            if (IsRunning)
            {
                return;
            }

            if (string.IsNullOrEmpty(config.server_ip))
            {
                OnError.Invoke("CloudSLAM server address is empty. Please set it in the CloudSLAMCamera.config object");
                return;
            }

            Screen.orientation = ScreenOrientation.LandscapeLeft; // Lock screen orientation to LandsscapeLeft, since we only support landscape.

            if (ShowVideoPreviewBackground)
            {
                javaCloudSLAM.Call("start", config.ToJavaCSConfig(), previewTextureWidth, previewTextureHeight, javaCallback);
            }
            else
            {
                javaCloudSLAM.Call("start", config.ToJavaCSConfig(), javaCallback);
            }
            poseBufferHandle = GetPoseBufferHandle();
        }

        /// <summary>
        /// Disconnect this CloudSLAMCamera from the CloudSLAM server.
        /// All resources consumed by the CloudSLAM client will be released.
        /// </summary>
        public void StopCloudSLAM()
        {
            if (!IsRunning)
            {
                return;
            }
            if (javaCloudSLAM != null)
            {
                javaCloudSLAM.Call("stop");
            }
        }

        /// <summary>
        /// Returns true if connected to the CloudSLAM server.
        /// </summary>
        /// <value><c>true</c> if connected to the CloudSLAM server; otherwise, <c>false</c>.</value>
        public bool IsRunning { get { return javaCloudSLAM != null ? javaCloudSLAM.Call<bool>("isRunning") : false; } }

        /// <summary>
        /// Resets the video preview background orientation according to the screen orientation.
        /// </summary>
        public void ResetOrientation()
        {
            SetupVideoPreviewMaterialParams();
        }

        void Update()
        {
            if (!poseBufferHandle.Equals(IntPtr.Zero))
            {
                Marshal.Copy(poseBufferHandle, poseBuffer, 0, 16);
                CloudSLAMCoordiantes.ApplyPoseToUnityTransform(poseBuffer, unityCamera.transform);
            }

        }

        void Start()
        {
            InitializeNativePlugin();

            if (OnOpen == null)
            {
                OnOpen = new UnityEvent();
            }

            if (OnClose == null)
            {
                OnClose = new UnityEvent();
            }

            if (OnTrackingStatusChanged == null)
            {
                OnTrackingStatusChanged = new CloudSLAMCameraTrackingStatusChangedEvent();
            }

            if (OnError == null)
            {
                OnError = new ClouddSLAMCameraErrorEvent();
            }

            unityCamera = GetComponent<Camera>();

            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext");

            javaCloudSLAM = new AndroidJavaObject("com.r1k.cloudslam.sdk.unityplugin.CloudSLAMForUnity", context);

            double fovY = javaCloudSLAM.Call<double>("getCameraFovYDegrees");
            unityCamera.fieldOfView = (float)fovY;

            javaCallback = new CloudSLAMFroUnityCallback(this);
        }

        void AddVideoPreview(Camera targetCamera)
        {
            SetupVideoPreviewMaterialParams();

            cameraEvent = targetCamera.actualRenderingPath.Equals(RenderingPath.Forward)
                ? CameraEvent.BeforeForwardOpaque
                : CameraEvent.BeforeGBuffer;

            buff = new CommandBuffer();
            buff.Blit(VideoPreviewTexture, BuiltinRenderTextureType.CurrentActive, videoPreviewMaterial);

            targetCamera.AddCommandBuffer(cameraEvent, buff);
        }

        void RemoveVideoPreview(Camera targetCamera)
        {
            targetCamera.RemoveCommandBuffer(cameraEvent, buff);
            buff = null;
        }

        void SetupVideoPreviewMaterialParams()
        {
            float targetWidth = previewTextureWidth;
            float targetHeight = previewTextureHeight;
            float widthScale = 1.0f;
            float heightScale = 1.0f;
            if (Screen.orientation.Equals(ScreenOrientation.LandscapeLeft) || Screen.orientation.Equals(ScreenOrientation.LandscapeRight))
            {
                targetWidth = Screen.width;
                targetHeight = (Screen.width * previewTextureHeight) / previewTextureWidth;
                heightScale = Screen.height / targetHeight;
            }
            else
            {
                targetHeight = Screen.height;
                targetWidth = (Screen.height * previewTextureHeight) / previewTextureWidth;
                widthScale = Screen.width / targetWidth;
            }

            videoPreviewMaterial.SetVector("_UvInfo", new Vector4(widthScale, heightScale, (int)Screen.orientation, 0.0f));
        }

        IEnumerator OnPreviewTexture(int ptr)
        {
            yield return new WaitForFixedUpdate();
            var texturePtr = new System.IntPtr(ptr);
            VideoPreviewTexture = Texture2D.CreateExternalTexture(previewTextureWidth, previewTextureHeight, TextureFormat.RGBA32, false, true, texturePtr);
            VideoPreviewTexture.Apply();
            AddVideoPreview(unityCamera);
        }

        IEnumerator OnOpenInternal()
        {
            yield return new WaitForFixedUpdate();
            OnOpen.Invoke();
        }

        IEnumerator OnCloseInternal()
        {
            yield return new WaitForFixedUpdate();
            RemoveVideoPreview(unityCamera);
            VideoPreviewTexture = null;
            poseBufferHandle = IntPtr.Zero;
            OnClose.Invoke();
        }

        IEnumerator OnErrorInternal(AndroidJavaObject e)
        {
            yield return new WaitForFixedUpdate();
            var errorString = e.Call<string>("toString");
            Debug.LogError(errorString);
            OnError.Invoke(errorString);
        }

        IEnumerator OnStatusChangedInternal(AndroidJavaObject status)
        {
            yield return new WaitForFixedUpdate();
            int asInt = status.Call<int>("val");
            var result = TrackingStatus.None;
            if (asInt == 2)
            {
                result = TrackingStatus.Tracking;
            }
            else if (asInt == 1)
            {
                result = TrackingStatus.Lost;
            }
            OnTrackingStatusChanged.Invoke(result);
        }

        private class CloudSLAMFroUnityCallback : AndroidJavaProxy
        {
            public CloudSLAMFroUnityCallback(CloudSLAMCamera parent) : base("com.r1k.cloudslam.sdk.unityplugin.CloudSLAMForUnity$Callback")
            {
                this.parent = parent;
            }

            public void onPreviewTextureAvailable(int previewTextureNativePtr)
            {
                parent.StartCoroutine(parent.OnPreviewTexture(previewTextureNativePtr));
            }

            public void onOpen()
            {
                parent.StartCoroutine(parent.OnOpenInternal());
            }

            void onStatusChanged(AndroidJavaObject status)
            {
                parent.StartCoroutine(parent.OnStatusChangedInternal(status));
            }

            void onCameraPoseUpdated()
            {
            }

            void onError(AndroidJavaObject e)
            {
                parent.StartCoroutine(parent.OnErrorInternal(e));
            }

            void onClose()
            {
                parent.StartCoroutine(parent.OnCloseInternal());
            }

            void onIdChanged(int id)
            {
            }

            void onBarcodeDetected(string barcode)
            {
            }

            private CloudSLAMCamera parent;
        }

        private static Matrix4x4 BuildCloudSLAMToUnityTransform()
        {
            Matrix4x4 mirrorY = Matrix4x4.identity;
            mirrorY[1, 1] = -1;
            return mirrorY;
        }
        
        [DllImport("cloudslam-unity-native")]
        private static extern void InitializeNativePlugin();

        [DllImport("cloudslam-unity-native")]
        private static extern IntPtr GetPoseBufferHandle();

        private Camera unityCamera;
        private AndroidJavaObject javaCloudSLAM;
        private CloudSLAMFroUnityCallback javaCallback;
        private CameraEvent cameraEvent;
        private CommandBuffer buff;
        private int previewTextureWidth = 1024;
        private int previewTextureHeight = 768;

        // Temp variables for OnPoseUpdateInternal() are created here, so that they are not constructed every Update();
        private Matrix4x4 cloudSlamToUnity = BuildCloudSLAMToUnityTransform();
        private Matrix4x4 poseMatrix = Matrix4x4.identity;
        private Vector3 position = Vector3.zero;
        private Vector3 direction = Vector3.forward;
        private Vector3 up = Vector3.up;

        private IntPtr poseBufferHandle = IntPtr.Zero;
        private float[] poseBuffer = new float[16];
        private ScreenOrientation originalScreenOrientation;
    }
    
    [System.Serializable]
    public class ClouddSLAMCameraErrorEvent : UnityEvent<string>
    {
    }
    
    [System.Serializable]
    public class CloudSLAMCameraTrackingStatusChangedEvent : UnityEvent<CloudSLAMCamera.TrackingStatus>
    {
    }
}
