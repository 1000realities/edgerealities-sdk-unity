using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using CloudSLAM;

public class CloudSLAMDemo : MonoBehaviour
{
    [Tooltip("A HUD element to display helper text")]
    public Text textBox;
    [Tooltip("An colored box used in indicate Tracking status. Red = Not connected, Yellow = Lost, Green = Tracking")]
    public Image statusBox;
    [Tooltip("Whether to fetch the Points of interest (Spheres, Cuboids and Anchors) from CloudSLAM. Fetching POIs requires the POIStorage component.")]
    public bool fetchPOIs;
    [Tooltip("Whether to fetch the map points from CloudSLAM. Fetching map points requires the CloudSLAMMap component.")]
    public bool fetchMap;
    
    public void OnCSOpen()
    {
        textBox.text = "Running";
        statusBox.color = new Color32(255, 182, 0, 255);
    }

    public void OnCSClosed()
    {
        statusBox.color = new Color32(255, 0, 21, 255);
        ShowHelpText();
    }

    public void OnCSStatusChanged(CloudSLAMCamera.TrackingStatus status)
    {
        switch(status)
        {
            case CloudSLAMCamera.TrackingStatus.Lost:
                statusBox.color = new Color32(255, 182, 0, 255);
                break;
            case CloudSLAMCamera.TrackingStatus.Tracking:
                statusBox.color = new Color32(0, 255, 72, 255);
                break;
        }
    }

    public void OnCSError(string error)
    {
        Debug.Log(error);
    }
    
    public void OnQRScanningStarted()
    {
        ShowText("Scanning QR code...");
        csCamera.gameObject.SetActive(false);
    }

    public void OnQRScanningFinished(string scanned)
    {
        csCamera.gameObject.SetActive(true);
        if (!string.IsNullOrEmpty(scanned))
        {
            UpdateConfigAddress(scanned);   
        }
        else
        {
            ShowHelpText();
        }
    }

    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        csCamera = FindObjectOfType<CloudSLAMCamera>();
        if (csCamera == null)
        {
            Debug.Log("CloudSLAM camera not found.");
            return;
        }

        if (PlayerPrefs.HasKey("CloudSLAM.demo.configAddress"))
        {
            configAddress = PlayerPrefs.GetString("CloudSLAM.demo.configAddress");
        }
        
        CloudSLAMConfig config = CloudSLAMConfig.LoadFromPlayerPrefs();
        if (config != null)
        {
            csCamera.config = config;
        }
        
        poiStorage = GetComponent<POIStorage>();
        if (fetchPOIs && !string.IsNullOrEmpty(configAddress))
        {
            StartCoroutine(FetchPOIsFromServer());
        }

        swipeObserver = new SwipeObserver();
        swipeObserver.Setup();

        ShowHelpText();
    }

    void Update()
    {
        var swipe = swipeObserver.CheckForSwipe();
        if (swipe.Equals(SwipeObserver.Swipe.Left))
        {
            Debug.Log("Swiped left");
            StartOrStopCloudSLAM();
        }
        if (swipe.Equals(SwipeObserver.Swipe.Right))
        {
            if (!csCamera.IsRunning)
            {
                FindObjectOfType<ConfigureWithQRCode>()?.StartOrStopScanningQRCode();   
            }
        }
    }
    
    private void StartOrStopCloudSLAM()
    {
        if (csCamera.IsRunning)
        {
            csCamera.StopCloudSLAM();
        }
        else
        {
            var cc = csCamera.config;

            csCamera.StartCloudSLAM();
            textBox.text = "Connecting...";
        }
    }
    
    private void ShowHelpText()
    {
        ShowText("Swipe right = scan QR code, left = start/stop");
    }

    private void ShowText(string text)
    {
        if (textBox != null)
        {
            textBox.text = text;   
        }
    }

    private void UpdateConfigAddress(string address)
    {
        configAddress = address;
        PlayerPrefs.SetString("CloudSLAM.demo.configAddress", configAddress);
        PlayerPrefs.Save();
        
        StartCoroutine(FetchConfigurationFromServer());
        if (fetchPOIs)
        {
            StartCoroutine(FetchPOIsFromServer());
        }
        if (fetchMap)
        {
            StartCoroutine(FetchMapFromServer());
        }
    }

    IEnumerator FetchConfigurationFromServer()
    {
        textBox.text = "Fetching configuration ...";
        UnityWebRequest www = CloudSLAMConfig.BuildGetConfigRequest(configAddress);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            textBox.text = "Configuration complete!";
            // Show results as text
            Debug.Log(www.downloadHandler.text);

            CloudSLAMConfig cc = JsonUtility.FromJson<CloudSLAMConfig>(www.downloadHandler.text);

            Debug.Log("server_ip: " + cc.server_ip);
            Debug.Log("websocket_port: " + cc.websocket_port);
            Debug.Log("framerate: " + cc.framerate);
            Debug.Log("bitrate: " + cc.bitrate);
            Debug.Log("interval: " + cc.keyframe_interval);
            Debug.Log("contrast: " + cc.contrast);

            csCamera.config = cc;
            cc.SaveToPlayerPrefs();
        }
    }

    IEnumerator FetchPOIsFromServer()
    {
        yield return poiStorage != null ? poiStorage.LoadPOIsFromServer(configAddress) : null;
    }

    IEnumerator FetchMapFromServer()
    {
        var map = GetComponent<CloudSLAMMap>();
        yield return map != null ? map.LoadMap(configAddress) : null;
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (csCamera != null)
        {
            csCamera.StopCloudSLAM();   
        }
    }

    private SwipeObserver swipeObserver;
    private CloudSLAMCamera csCamera;
    private string configAddress;
    private POIStorage poiStorage;
}

