# Edge Realities Unity SDK + example

This repository contains the Unity 3D client SDK example for the core Edge Realities platform by [1000 realities](http://1000realities.io) (formerly known as CloudSLAM). Edge Realities is a framework for creating 6DoF tracking and augmented reality experiences using a remote server. 
**Please note that while this repository contains the client SDK, an Edge Realities server instance is necessary to operate it. For more information on how to obtain a license for an Edge Realities server instance please contact <info@1000realities.io>.**

## Demo video
A highlight of the Edge realities platform's core AR and tracking capabilities can be found here: <https://youtu.be/7qFAk6t288A>

For more videos please visit the [1000 realities youtube channel](https://www.youtube.com/channel/UCHrD8Ytr5FwLUt706l8dzIQ)

## Getting started
1. Obtain an Edge Realities server license. For more information please contact <info@1000realities.io>
2. Download & install [Unity](https://unity.com/) 2018.3 or higher.
3. Clone the repository.
4. Open the project (i.e. the repository's root folder) in Unity.
5. Build the example app and run it on your device of choice. (See below for a list of currently supported devices).

## Using the example app
### 1. Start the Edge Realities server instance
1. In your web browser, navigate to the Edge Realities server instance URL provided along with your license.
2. Log to the admin panel website with the credentials provided along with your license.
3. You should be redirected to the home page of the Edge Realities' platform admin panel.
![home page](https://raw.githubusercontent.com/1000realities/edgerealities-sdk-android/master/doc/images/admin-panel-home.png)
4. On the home page, start the 6DoF tracking server module by pressing the "Restart CloudSLAM" button.
5. After a short while the "CloudSLAM status" panel will change to green and the value will be "Running". 

### 2. Connect the client app
1. Run the example app on your device. If the application is ran for the first time it is important that you **accept all permission requests**.
2. Swipe the screen left to show the QR code scanner view, and point the device to the configuration QR code show on the home page of the Edge Realities admin panel (in your web browser).
3. The application should return to the main screen once it was successfully configured for usage with Edge Realities.
4. Swipe right to connect/disconnect to the Edge Realities server intsance.

### 3. Initialize and build an environment map
1. Once connected, the "video" section (or "Map" section) of the Edge Realities administration panel should show a live preview from the camera on the device.
2. In no environment data is present, red lines will be displayed over the preview indicating that the system is trying to initialize environment tracking.
![env init](https://raw.githubusercontent.com/1000realities/edgerealities-sdk-android/master/doc/images/env-initialize.png)
3. To initialize the environment data, move your device around the environment while looking at the video preview. Try to avoid rapid movement, and make sure that the device is changing it's position (i.e. not just rotating).
4. After a few seconds, on the video preview you will see that the red lines have been replaced by green dots. This means that the envionrment data has been initialized tracking is in progress
![env tracking](https://raw.githubusercontent.com/1000realities/edgerealities-sdk-android/master/doc/images/env-tracking.png)
5. Note: The initialization process can take anywhere from a few seconds to 15 minutes (depending on the environment, more clutter = faster initialization), so please be patient.
6. Move your device around the environment to map it. Try to capture different viewing angles to obtain well defined environment data. You can see the data being built up live in the "Map" section.
7. Once happy with the results, use the "Restart CloudSLAM" button on the home page to save your environment.
8. In case you don't want to map the environment further and wish to focus only on tracking your device, building the environment map can be disabled in the "Map" section.

## SDK core concepts / TL;DR
1. The CloudSLAMCamera script represents the core of the SDK. Attaching it to your main camera should be your starting point.
2. The CloudSLAMCamera.StartCloudSLAM() method is used to connect the the EdgeRealities server instance and commence environment tracking/mapping.
3. The CloudSLAMConfig property of the CloudSLAMCamera component may be filled in manually, however the example app provides a means of automatic configuration through scanning a QR code.
4. Use the events provided in the CloudSLAMCamera component to fetch relevant events from the SDK.
5. If you want the video stream to be previewed on your device, check the "Show video preview background" checkbox in the CloudSLAMCamera component (checked by default).
6. Use CloudSLAMCamera.StopCloudSLAM() to disconnect from the Edge Realities server instance.
7. The Edge Realities unity SDK requires the native android SDK stored in /Assets/CloudSLAM/Plugins/Android.

## SDK Documentation

The full Edge Realities SDK doc can be found at <http://1000realities.io/docs/cloudslam/sdk/unity/index.html>.

## Device support
### Currently supported devices
- Smart phones
   - Samsung Galaxy S8
   - Samsung Galaxy S7
   - Samsung Galaxy S5
   - Google Pixel 1
   - Motorola G6
   - MyPhone Fun 18x9
- Samrt glasses
   - Vuzix M300
   - Vuzix M300XL
   - Vuzix Blade
   - RealWear HMT-1
   - RealMax quian
   - Epson Moverio BT 300

### Support for other devices
Any android device may be supported by Edge Realities provided it meets the following minimal requirements:
- Android OS 5.1 or higher.
- At least 1 RGB camera capable of delivering 640x480 video at 30 fps.
- Supported H.264 video encoder/decoder.
- Some form of connectivity, e.g. a Wi-Fi module, 5G modem etc.
- Optional: IMU (accelerometer + gyroscope).

Each device model must be calibrated for camera intrinsics (and other parameters) prior to onboarding to the Edge Realities platform. Currently calibration is carried out by the 1000 realities team. 
We are planning to enable end-users to calibrate their own devices in the future.

Would you like Edge Realities to support your device? Contact us: <info@1000realities.io>
