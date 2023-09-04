![Screenshot 2023-09-04 at 12 54 11 PM](https://github.com/eundersander/siro_hitl_unity_client/assets/6557808/5aac5731-a7eb-4f0d-876c-871b330f6677)
*Screenshot of the Unity VR client (left) running on a dev machine, with VR emulation, alongside the HITL server (right). The VR headset and controller poses are visualized at right as a view frustum (white lines) and red/blue boxes.*

# Installation
We've tested these steps on a Macbook and a Linux machine.

1. Don't install the Unity Editor yet! The steps below will help guide you to the correct version.
2. Install the Unity *Hub* (from https://unity.com/download)
3. Clone this project (git@github.com:eundersander/siro_hitl_unity_client.git)
4. Try to open the project in UnityHub.
* this will trigger installation of the correct version of the Unity Editor. As of Aug 2023, this is `2022.3.7f1`.
5. At the start of the Unity installation, you're asked about platforms to install.
* Select "Android Built Tools", including subitems "OpenJDK" and "Android SDK and NDK Tools".
6. After the correct Unity Editor version is installed, open the siro_hitl_unity_client project again from UnityHub.
* You might hit errors related to XCode and GLTF importer. In my case, I just needed to agree to install something related to XCode and then retry opening the siro_hitl_unity_client project.
7. Once the project is open in the Unity Editor, open the correct Scene: `File > Open Scene > Assets > Scenes > GfxReplayPlayerScene`
8. Install external data (see below).

# Installing external SIRo data
GLTF assets for the SIRo client aren't included in the Unity project (aside from a test asset in `Assets/temp/`). They must be imported separately. These are built using a special pipeline that isn't defined here (documentation coming soon!). A copy of this data is currently [here](https://drive.google.com/file/d/1NkDI2kLoTFjr5eEhtzQn2D1jCCdAh6_m/view?usp=drive_link). Let's assume you've downloaded this data to `path/to/habitat-lab/data/hitl_simplified/data` (outside the siro_hitl_unity_client project). Once this is done, this entire data folder needs to be imported into the Unity project, and this can be done via a script invoked from the Menu Bar. Tools > Update Data Folder... > select the external location of your data folder. Running this command will import all GLB files. They live at `Assets/Resources/data`. Running this script is the same as drag-and-dropping the data folder from Mac Finder to the Unity Editor Project Pane, under `Assets/`. You can inspect the imported assets in the Unity Editor by browsing the data folder in the Project pane.

# Local testing on your dev machine
No HITL server or VR headset is required for this.
1. Test opening a GLB file.
* Drag from Project pane > `Assets/temp/106879104_174887235.glb` into the scene, then delete this game object when you're done testing.
2. Test GfxReplayPlayer with a local gfx-replay json file. Conceptually, this tries to spawn and pose GameObjects with names corresponding to the render asset instances in the json, and these GameObjects should be found in the externally-imported SIRo data (above). 
* In the Hierarchy Pane, select the `ScriptSingletonsObject` object.
* In the Inspector Pane, enable the `Replay File Loader` script and disable the `Network Client` script. Conceptually, you are switching the app to use a local source of gfx-replay keyframes (the json file) instead of receiving them from the HITL server.
* Again in ScriptSingletonsObject's Inspector Pane, under "Replay File Loader", ensure you have a valid Json File set. There are a few available in `Assets/temp`. You can import new gfx-replay json files into the project by drag-and-dropping json files into Project Pane > Assets > temp.
* Note the scene's `XR Device Simulator` game object (disabled by default). Note also ScriptSingletonsObject's `XR Input Helper Component`. This component automatically enables the XR Device Simulator object when running in the editor (this simulator must not be enabled when running on the VR headset).
* Hit play and browse the scene.
    * Use WASD, Q/E, and the mouse to move and look around.
    * You can also try teleporting, but this requires some familiarity with the XR Device Simulator (see on-screen help display plus help videos on the web).
    * Hold Spacebar to advance through keyframes; you may see the Spot robot move. This logic lives in `ReplayFileLoader`.
    * Note you can't interact with the scene; interaction functionality requires the HITL server (below).

# Local testing on a Quest VR headset
No HITL server is required for this.

1. Quest is an Android device. Open Menu bar > Build Settings. From the platform list, select "Android". Click "Switch Platform" and wait a minute for the Editor to switch platforms.
2. Plug in your Quest to your dev machine via USB. Ensure Developer Mode. You may need to install ADB or other Android Dev tools (anecdotally, this isn't required on Mac).
3. Still in the Build Settings window, next to "Run Device", click "Refresh", then look for your specific device in the dropdown menu. Select it.
4. Click "Build and Run" and ensure this completes without error. If you're prompted for a build save location, just choose `build.apk` in the project root folder.
5. Put on your headset. The app may already be running. If not, or if you encounter issues, hit the Quest home button and it should let you quit the `siro_hitl_vr_client` program. After you quit, you can usually find the icon for the program in your list of recently-run programs. You should re-launch it.

# Testing with the HITL server
1. Undo any local changes to `ScriptSingletonsObject` in the Inspector Pane. In particular, make sure `ReplayFileLoader` is disabled and `NetworkClient` is enabled.
2. Under `Network Client`, enter the server address and port where you'll run the HITL server.
3. Follow instructions for [Testing the HITL server and Unity VR client together](https://docs.google.com/document/d/1cvKuXXE2cKchi-C_O7GGVFZ5x0QU7J9gHTIETzpVKJU/edit#bookmark=id.l3y1pdpos6t).
4. See also [Playing Fetch in the VR client](https://docs.google.com/document/d/1cvKuXXE2cKchi-C_O7GGVFZ5x0QU7J9gHTIETzpVKJU/edit#bookmark=id.uqh8z8qcn7y8) for tips on what to do in the scene.

# Troubleshooting
* Beware you may see a runtime error in the Unity Console about a null reference inside XR Device Input code. I haven't tracked this down, but it appears to be a bug with the XR Device Simulator and it appears to be harmless. However, it should be fixed soon because it is causing console spam and potentially masking important errors.
* If you have problems getting the client to connect to the server, one thing to test is whether a minimal client can connect to the websocket connection. This minimal client can be a simple python app or simple HTML/Javascript page (the latter can be served from the server via a HTTP server and loaded in the Quest browser). Ask Eric U for instructions.
