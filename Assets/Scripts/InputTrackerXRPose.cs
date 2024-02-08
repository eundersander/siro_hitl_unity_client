using UnityEngine;

public class InputTrackerXRPose : InputTracker
{
    AvatarData _inputData = new AvatarData();

    public GameObject xrHeadObject;
    public GameObject xrLeftControllerObject;
    public GameObject xrRightControllerObject;

    public override void UpdateClientState(ref ClientState state)
    {
        _inputData.root.FromGameObject(xrHeadObject);
        _inputData.hands[0].FromGameObject(xrLeftControllerObject);
        _inputData.hands[1].FromGameObject(xrRightControllerObject);
        state.avatar = _inputData;
    }

    public override void OnEndFrame() {}
}
