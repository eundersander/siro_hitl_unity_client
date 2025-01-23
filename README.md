# Habitat VR HITL Client
![spot_and_hand_banner](https://github.com/user-attachments/assets/9d60447b-ed2e-4460-aaa7-a4c2a72a55cf)

This application is a VR client for use with [client/server Habitat HITL applications](https://github.com/facebookresearch/habitat-lab/tree/main/habitat-hitl). For this branch `eundersander/isaac_vr2`, the VR client has been modified to work with our [VR physics sandbox](https://github.com/facebookresearch/habitat-lab/tree/eundersander/isaac_vr/examples/hitl/isaacsim_viewer); specifically, we've added hand-tracking.

# Installation and Testing

1. See [the README in branch `main`](https://github.com/eundersander/siro_hitl_unity_client/blob/main/README.md) for general tips on installing Unity, testing the client, and deploying to Quest.
2. Download [vr_sandbox_unity_resources_data.zip](https://drive.google.com/file/d/1jw160hyFolOo6lfGf4Z9GA_ZFI5w7LBJ/view?usp=sharing) and extract to `Assets/Resources` so that you have subfolders like `Assets/Resources/data/from_gum`.
3. Open the Unity project, open the PlayerVR scene, and switch to the Android platform.
4. Set your server IP address from the Unity Editor: from Hierarchy pane, select the `App` object. From Inspector pane, find `Config Loader (Script)` attached to this object. Modify `Default Server Locations`.
5. Hit play to run the client inside Unity. At the same time, start your [HITL server](https://github.com/facebookresearch/habitat-lab/tree/eundersander/isaac_vr/examples/hitl/isaacsim_viewer). The client will eventually connect to the server and the house scene will appear. The in-editor VR emulation doesn't work well; for in-editor testing, I recommend switching from the Game pane to the Scene pane. Then, select `OVRCameraRig` from the Hierarchy pane and use the Move Tool in the Scene pane; you're able to move the Metahand's base in this way, e.g. you can knock over objects or bump the Spot robot.

# Hand-tracking Details
In our scene, we've replaced the controller fabs with hand fabs from the Meta Unity package (see `OVRCameraRig`), and we forward the hand poses to the server to enable server-side hand-tracking logic (Metahand teleop). Because we've replaced the controller fabs with hands, controller buttons aren't usable in this scene. In the future, we should modify our Unity scene to support auto-switching between hands and controller fabs.
