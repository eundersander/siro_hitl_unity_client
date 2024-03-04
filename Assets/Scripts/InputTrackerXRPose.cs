using UnityEngine;

public class InputTrackerXRPose : IClientStateProducer
{
    AvatarData _inputData = new AvatarData();
    GameObject _xrHead;
    GameObject _xrLeftController;
    GameObject _xrRightController;

    public InputTrackerXRPose(GameObject xrHead, GameObject xrLeftController, GameObject xrRightController)
    {
        _xrHead = xrHead;
        _xrLeftController = xrLeftController;
        _xrRightController = xrRightController;
    }

    public void UpdateClientState(ref ClientState state)
    {
        _inputData.root.FromGameObject(_xrHead);
        _inputData.hands[0].FromGameObject(_xrLeftController);
        _inputData.hands[1].FromGameObject(_xrRightController);
        state.avatar = _inputData;
    }

    public void OnEndFrame() {}

    public void Update() {}
}
