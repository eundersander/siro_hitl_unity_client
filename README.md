![Screenshot 2023-09-04 at 12 54 11 PM](https://github.com/eundersander/siro_hitl_unity_client/assets/6557808/5aac5731-a7eb-4f0d-876c-871b330f6677)
*Screenshot of the Unity Editor (left) running on a dev machine, with VR emulation, alongside the HITL server (right). The VR headset and controller poses are visualized at right as a view frustum (white lines) and red/blue boxes.*

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
GLTF assets for the SIRo client aren't included in the Unity project (aside from a test asset in `Assets/temp/`). They must be imported separately. These are built using a special pipeline that isn't defined here (documentation coming soon!). The latest version of this data is [here](https://drive.google.com/file/d/1r20vpmCT5kWas6HtMpmYIPhm3Ww-CHi1/view) and an older copy is [here](https://drive.google.com/file/d/1NkDI2kLoTFjr5eEhtzQn2D1jCCdAh6_m/view?usp=drive_link). Let's assume you've downloaded this data to `path/to/habitat-lab/data/hitl_simplified/data` (outside the siro_hitl_unity_client project). Once this is done, this entire data folder needs to be imported into the Unity project, and this can be done via a script invoked from the Menu Bar. Tools > Update Data Folder... > select the external location of your data folder. Running this command will import all GLB files. They live at `Assets/Resources/data`, for example `Assets/Resources/data/fpss/stages/106879104_174887235.glb`. Running this script is the same as drag-and-dropping the data folder from Mac Finder to the Unity Editor Project Pane, under `Assets/`. You can inspect the imported assets in the Unity Editor by browsing the data folder in the Project pane.

# Local testing with the Unity Editor and VR emulation
In this step, you'll view an HSSD stage model, get familiar with the Unity Editor, and learn tVR emulation controls. No HITL server or VR headset is required. You can navigate around the scene but there's no interactivity (interactivity requires the HITL server, below).

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

# More local testing: viewing an HSSD scene with ReplayFileLoader
In this step, you'll play a pre-recorded gfx-replay json file. You'll see a full HSSD scene with clutter and an animating Spot robot.

1. In the Hierarchy Pane, select the `ScriptSingletonsObject` object.
2. In the Inspector Pane, enable the `Replay File Loader` script and disable the `Network Client` script.
    * Conceptually, you are switching the app to use a local source of gfx-replay keyframes (the json file) instead of receiving them from the HITL server. The `Replay File Loader` script spawns and poses GameObjects with names corresponding to the render asset instances in the json, and these GameObjects should be found in the externally-imported SIRo data (above). 
3. Again in ScriptSingletonsObject's Inspector Pane, under "Replay File Loader", ensure you have a valid Json File set. There are a few available in `Assets/temp`. Optional: you can also import new gfx-replay json files into the project by drag-and-dropping json files into Project Pane > Assets > temp.
4. Hit play and browse the scene.
    * Hold Spacebar to advance through keyframes; you may see the Spot robot move. This logic lives in `ReplayFileLoader`.

# Testing on a Quest VR headset
In this step, we'll view the same HSSD scene on the Quest VR headset. No HITL server is required for this.

1. Quest is an Android device. Open Menu bar > Build Settings. From the platform list, select "Android". Click "Switch Platform" and wait a minute for the Editor to switch platforms.
2. Plug in your Quest to your dev machine via USB. Ensure Developer Mode. You may need to install ADB or other Android Dev tools (anecdotally, this isn't required on Mac).
3. Still in the Build Settings window, next to "Run Device", click "Refresh", then look for your specific device in the dropdown menu. Select it.
4. Click "Build and Run" and ensure this completes without error. If you're prompted for a build save location, just choose `build.apk` in the project root folder.
5. Put on your headset. The app may already be running. If not, or if you encounter issues, hit the Quest home button and it should let you quit the `siro_hitl_vr_client` program. After you quit, you can usually find the icon for the program in your list of recently-run programs. You should re-launch it.
6. When in the scene, press and hold forward on either VR controller thumbstick to aim for a teleportation destination, then release the thumbstick to teleport.

# Testing the interactive Fetch task with the HITL server
This is the full, interactive demo. You can use either the Unity Editor (VR emulation) or the Quest VR headset. You can run the Unity Editor on the same machine as the HITL server, side-by-side--this is shown in the screenshot above.

1. Undo any local changes to `ScriptSingletonsObject` in the Inspector Pane. In particular, make sure `ReplayFileLoader` is disabled and `NetworkClient` is enabled.
    * Remember, you can always do `git status` in the project root folder to check if you've accidentally made unwanted local changes (caveat: unsaved changes probably won't show up here, so remember to save often).
3. Under `Network Client`, enter the server address and port where you'll run the HITL server. (This is probably the same machine where you're running the Unity Editor.)
4. Follow instructions for [Testing the HITL server and Unity client together](https://docs.google.com/document/d/1cvKuXXE2cKchi-C_O7GGVFZ5x0QU7J9gHTIETzpVKJU/edit#bookmark=id.l3y1pdpos6t).
5. See also [Playing Fetch in the VR client](https://docs.google.com/document/d/1cvKuXXE2cKchi-C_O7GGVFZ5x0QU7J9gHTIETzpVKJU/edit#bookmark=id.uqh8z8qcn7y8) for tips on what to do in the scene.

# Troubleshooting
* Beware you may see a runtime error in the Unity Console about a null reference inside XR Device Input code. I haven't tracked this down, but it appears to be a bug with the XR Device Simulator and it appears to be harmless. However, it should be fixed soon because it is causing console spam and potentially masking important errors.
* You may see some errors about missing GLB models. In some cases, this is expected, e.g. we've intentionally omitted a high-poly model. In other cases, you might have set up your data incorrectly. In the Project pane, make sure you have e.g. `Assets/Resources/data/fpss/stages/106879104_174887235.glb`.
* If you have problems getting the client to connect to the server, one thing to test is whether a minimal client can connect to the websocket connection. This minimal client can be a simple python app or simple HTML/Javascript page (the latter can be served from the server via a HTTP server and loaded in the Quest browser). Ask Eric U for instructions.
