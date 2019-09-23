using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ConfigureWithQRCode : MonoBehaviour
{
    public GameObject QRDecodeControllerPrefab;
    public GameObject QRCameraPrefab;

    public UnityEvent scanningStarted;
    public QSScanningFinishedEvent scanningFinished;
    
    public void StartOrStopScanningQRCode()
    {
        if (!scanning)
        {
            scanningStarted.Invoke();
            
            cameraQR.SetActive(true);
            qr_controler.StartWork();
            scanning = true;
        }
        else
        {
            qr_controler.StopWork();
            cameraQR.SetActive(false);
            scanning = false;
            
            scanningFinished.Invoke(null);
        }
    }

    void Start()
    {
        if (scanningStarted == null)
        {
            scanningStarted = new UnityEvent();
        }

        if (scanningFinished == null)
        {
            scanningFinished = new QSScanningFinishedEvent();
        }

        cameraQR = Instantiate(QRCameraPrefab);
        cameraQR.SetActive(false);
        qr_controler = Instantiate(QRDecodeControllerPrefab).GetComponent<QRCodeDecodeController>();
        qr_controler.e_DeviceController = cameraQR.GetComponent<DeviceCameraController>();
        qr_controler.onQRScanFinished += onScanFinished;
        
        scanning = false;
    }

    void onScanFinished(string str)
    {
        Debug.Log("onScanFinished str: " + str);
        qr_controler.StopWork();

        // return to 3D camera.
        cameraQR.SetActive(false);
        scanning = false;

        scanningFinished.Invoke(str);
    }

    private GameObject cameraQR;
    private QRCodeDecodeController qr_controler;
    private bool scanning;
}

[System.Serializable]
public class QSScanningFinishedEvent : UnityEvent<string>
{
}
