using System;
using UnityEngine;
using UnityEngine.Networking;

namespace CloudSLAM
{
    [System.Serializable]
    public class CloudSLAMConfig
    {
        /// <summary>
        /// The IP (or host name) of the CloudSLAM server.
        /// </summary>
        public string server_ip;
        /// <summary>
        /// The port of the CloudSLAM server's data channel.
        /// When multiple clients are connected, the data port
        /// will be assigned automatically.
        /// </summary>
        public int udp_port = 8888;
        /// <summary>
        /// The port of the CloudSLAM server's control channel.
        /// When multiple clients are connected, the control port
        /// is shared between all clients.
        /// </summary>
        public int websocket_port = 8889;
        /// <summary>
        /// The video streaming framerate. Should be set according to the 
        /// device specs provided by 1000 realities.
        /// </summary>
        public int framerate = 30;
        /// <summary>
        /// The communication protocol with the CloudSLAM server. 
        /// Should be set to 4.
        /// </summary>
        public int protocol = 4;
        /// <summary>
        /// The width the video stream set to the ClientSLAM server.
        /// Should be set to 640.
        /// </summary>
        public int res_width = 640;
        /// <summary>
        /// The height the video stream set to the ClientSLAM server.
        /// Should be set to 480.
        /// </summary>
        public int res_height = 480;
        /// <summary>
        /// The bitrate of the video stream to the CloudSLAM server.
        /// Should be set according to the device specs provided
        /// by 1000 realities.
        /// </summary>
        public int bitrate = 1500000;
        /// <summary>
        /// The keyframe interval of the video stream. Should be set according
        /// to the device specs provided by 1000 realities.
        /// </summary>
        public int keyframe_interval = 0;
        /// <summary>
        /// Rotation of the video frames sent to the CloudSLAM server.
        /// Should be set to 0.
        /// </summary>
        public int frame_rotation = 0;
        /// <summary>
        /// Contract of the video frames sent to the CloudSLAM server.
        /// 128 is the unmodified contrast value.
        /// </summary>
        public int contrast = 128;

        /// <summary>
        /// Loads a CloudSLAMConfig from Unity PlayerPrefs.
        /// </summary>
        /// <returns>The from player prefs.</returns>
        public static CloudSLAMConfig LoadFromPlayerPrefs()
        {
            if (!PlayerPrefs.HasKey("CloudSLAM.ServerIP"))
            {
                return null;
            }

            var config = new CloudSLAMConfig();
            config.server_ip = PlayerPrefs.GetString("CloudSLAM.ServerIP");
            config.udp_port = PlayerPrefs.GetInt("CloudSLAM.UDPPort");
            config.websocket_port = PlayerPrefs.GetInt("CloudSLAM.WebsocketPort");
            config.protocol = PlayerPrefs.GetInt("CloudSLAM.Protocol");
            config.framerate = PlayerPrefs.GetInt("CloudSLAM.Framerate");
            config.bitrate = PlayerPrefs.GetInt("CloudSLAM.Bitrate");
            config.keyframe_interval = PlayerPrefs.GetInt("CloudSLAM.Interval");
            config.contrast = PlayerPrefs.GetInt("CloudSLAM.Contrast");
            config.res_width = PlayerPrefs.GetInt("CloudSLAM.SentFrameWidth");
            config.res_height = PlayerPrefs.GetInt("CloudSLAM.SentFrameHeight");
            config.frame_rotation = PlayerPrefs.GetInt("CloudSLAM.SentFrameRotation");

            return config;
        }

        /// <summary>
        /// Builds the get configuration request to the CloudSLAM server at the 
        /// given URL.
        /// </summary>
        /// <returns>The get configuration request.</returns>
        /// <param name="serverUrl">The CloudSLAM Server URL.</param>
        public static UnityWebRequest BuildGetConfigRequest(string serverUrl)
        {
            return UnityWebRequest.Get("http://" + serverUrl + "/client/config");
        }

        /// <summary>
        /// Produces a native CSConfig object.
        /// </summary>
        /// <returns>The native CSConfig object.</returns>
        public AndroidJavaObject ToJavaCSConfig()
        {
            var protocolClass = new AndroidJavaClass("com.r1k.cloudslam.sdk.CSConfig$Protocol");
            var rotationClass = new AndroidJavaClass("com.r1k.cloudslam.sdk.CSConfig$Rotation");
            var javaCSConfig = new AndroidJavaObject("com.r1k.cloudslam.sdk.CSConfig");

            javaCSConfig.Set<string>("ip", server_ip);
            javaCSConfig.Set<int>("udpPort", udp_port);
            javaCSConfig.Set<int>("webSocketPort", websocket_port);
            javaCSConfig.Set<AndroidJavaObject>("protocol", protocolClass.CallStatic<AndroidJavaObject>("getEnum", protocol));
            javaCSConfig.Set<int>("frameRate", framerate);
            javaCSConfig.Set<int>("bitrate", bitrate);
            javaCSConfig.Set<int>("keyFrameInterval", keyframe_interval);
            javaCSConfig.Set<int>("contrast", contrast);
            javaCSConfig.Set<int>("frameWidth", res_width);
            javaCSConfig.Set<int>("frameHeight", res_height);
            javaCSConfig.Set<AndroidJavaObject>("frameRotation", rotationClass.CallStatic<AndroidJavaObject>("getEnum", frame_rotation));
            // Camera color is always true since we need it for the video preview.
            javaCSConfig.Set<bool>("cameraColor", true);

            return javaCSConfig;
        }

        /// <summary>
        /// Saves this CloudSLAMConfig to Unity PlayerPrefs.
        /// </summary>
        public void SaveToPlayerPrefs()
        {
            PlayerPrefs.SetString("CloudSLAM.ServerIP", server_ip);
            PlayerPrefs.SetInt("CloudSLAM.UDPPort", udp_port);
            PlayerPrefs.SetInt("CloudSLAM.WebsocketPort", websocket_port);
            PlayerPrefs.SetInt("CloudSLAM.Protocol", protocol);
            PlayerPrefs.SetInt("CloudSLAM.Framerate", framerate);
            PlayerPrefs.SetInt("CloudSLAM.Bitrate", bitrate);
            PlayerPrefs.SetInt("CloudSLAM.Interval", keyframe_interval);
            PlayerPrefs.SetInt("CloudSLAM.Contrast", contrast);
            PlayerPrefs.SetInt("CloudSLAM.SentFrameWidth", res_width);
            PlayerPrefs.SetInt("CloudSLAM.SentFrameHeight", res_height);
            PlayerPrefs.SetInt("CloudSLAM.SentFrameRotation", frame_rotation);
            PlayerPrefs.Save();
        }
    }
}
