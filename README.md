# Habitat VR HITL Client

This application is a VR client that can remotely control a human agent in Habitat, in a human-in-the-loop (HITL) evaluation context.

See [this document](https://github.com/facebookresearch/habitat-lab/blob/main/examples/hitl/pick_throw_vr/README.md#vr) for setting up the server. This is a "thin client", with most application complexity residing on the server. However, the client can be tested alone (without the server) as a simple model viewer, and we recommend this as a sanity check before running the full client/server application. See local testing instructions below.

![Screenshot](https://github.com/eundersander/siro_hitl_unity_client/assets/6557808/5aac5731-a7eb-4f0d-876c-871b330f6677)
*Screenshot of the Unity Editor (left) running on a dev machine, with VR emulation, alongside the HITL server (right). The VR headset and controller poses are visualized at right as a view frustum (white lines) and red/blue boxes.*

---

- [Installation](#installation)
- [Local testing with the Unity Editor and VR emulation](#local-testing-with-the-unity-editor-and-vr-emulation)
- [Testing on a Quest VR headset](#testing-on-a-quest-vr-headset)
- [Testing with the HITL server](#testing-with-the-hitl-server)
  - [Installing external Habitat data](#installing-external-habitat-data)
- [Troubleshooting](#troubleshooting)
  - [Deployment to the VR Headset](#deployment-to-the-vr-headset)
  - [Other Issues](#other-issues)

## Installation
Tested on Linux and MacOS.

1. Install the *Unity Hub* (from https://unity.com/download)
2. Clone this repository.
3. Try to open the project in Unity Hub.
    * This will trigger installation of the correct version of the Unity Editor (`2022.3.7f1`).
4. At the start of the Unity installation, you're asked about platforms to install.
   * Select `Android Build Tools`, including subitems `OpenJDK` and `Android SDK and NDK Tools`.
5. After the correct Unity Editor version is installed, open the `siro_hitl_unity_client project` again from Unity Hub.
6. Once the project is open in the Unity Editor, open the correct scene: `File > Open Scene > Assets > Scenes > GfxReplayPlayerScene`

## Local testing with the Unity Editor and VR emulation
In this step, you'll view an HSSD stage model, get familiar with the Unity Editor, and learn the VR emulation controls. No HITL server or VR headset is required. You can navigate around the scene but there's no interactivity (interactivity requires the HITL server, below).

1. From the Project pane, navigate the tree to `Assets/temp`. Drag `106879104_174887235` into the Scene pane. You should see an HSSD stage with walls and floors but no other clutter.
2. In the Scene pane, hold Alt + Left Click and drag the mouse to rotate your view. Check some videos or tutorials on the web to learn more Unity Editor controls.
3. Note the scene's `XR Device Simulator` game object (disabled by default). Note also ScriptSingletonsObject's `XR Input Helper Component`. This component automatically enables the XR Device Simulator object when running in the editor (this simulator must not be enabled when running on the VR headset).
4. Hit play (top-center button). After some loading, you should see a first-person view of the stage, along with simulated VR controllers.
5. Browse the scene.
    * Use WASD, Q/E, and the mouse to move and look around.
    * You can also try teleporting, but this requires some familiarity with the XR Device Simulator (see on-screen help display plus help videos on the web).
    * Check out both the "Scene" and "Game" tabs.
    * Note you can't interact with the scene; interaction functionality requires the HITL server (below).
6. When you're done testing, hit play again to stop.
7. In the Hierarchy pane, find `106879104_174887235` (the object you added earlier). Delete it.

## Testing on a Quest VR headset
In this step, we'll view the same HSSD stage on the Quest VR headset. No HITL server is required for this.

1. Quest is an Android device. Open Menu bar > Build Settings. From the platform list, select "Android". Click "Switch Platform" and wait a minute for the Editor to switch platforms.
2. Plug in your Quest to your dev machine via USB. Ensure Developer Mode. You may need to install ADB or other Android Dev tools (anecdotally, this isn't required on Mac).
3. Still in the Build Settings window, next to "Run Device", click "Refresh", then look for your specific device in the dropdown menu. Select it.
4. Click "Build and Run" and ensure this completes without error. If you're prompted for a build save location, just choose `build.apk` in the project root folder.
5. Put on your headset. The app may already be running. If not, or if you encounter issues, hit the Quest home button and it should let you quit the `siro_hitl_vr_client` program. After you quit, you can usually find the icon for the program in your list of recently-run programs. You should re-launch it.
6. When in the scene, press and hold forward on the right VR controller thumbstick to aim for a teleportation destination, then release the thumbstick to teleport.

See [these troubleshooting steps](#deployment-to-the-vr-headset) if you have issues deploying the client to your VR device.

## Testing with the HITL server
[This document](https://github.com/facebookresearch/habitat-lab/blob/main/examples/hitl/pick_throw_vr/README.md#vr) contains complete information for setting up the server.

### Installing external Habitat data
The `Assets/Resources/data` folder is a subset of the Habitat `data/` folder. When the Habitat server indicates that a file must be loaded, the Unity client will look for an asset with the same name within its own data folder.

Assets for the Habitat client aren't included in the Unity project (aside from a test asset in `Assets/temp/`). They must be imported separately. These are built using a special pipeline defined [here](https://github.com/facebookresearch/habitat-lab/blob/main/examples/hitl/pick_throw_vr/README.md#dataset-processing).

## Troubleshooting
### Deployment to the VR Headset

* Make sure that [developer mode](https://developer.oculus.com/documentation/native/android/mobile-device-setup/) is activated.
* When connecting your headset to your development computer via USB, a pop-up will ask you to confirm the connection within the VR headset.
  * If the pop-up doesn't show up, reconnect your USB cable.
  * You may also have to re-do this after the headset goes into sleep mode.
* Deployment occasionally fails when the application is already installed. You can delete the old build from the Quest storage menu (find it under 'unknown sources'). The following error will often show in Unity when that occurs:
```
CommandInvokationFailure: Unable to install APK to device. Please make sure the Android SDK is installed and is properly configured in the Editor. See the Console for more details.
/home/user/Unity/Hub/Editor/2022.3.7f1/Editor/Data/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb -s "4G3YA1ZF571D4D" install -r -d "/home/user/git/siro_hitl_unity_client/Build/build.apk"
```
### Other Issues
* You may see some errors about missing GLB models. In some cases, this is expected, e.g. we've intentionally omitted a high-poly model. In other cases, you might have set up your data incorrectly. The asset path within `Assets/Resources/data` must mirror your `habitat-lab` `/data` folder.
