using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ConfigLoader))]
public class App : MonoBehaviour
{
    protected GfxReplayPlayer _gfxReplayPlayer;
    protected NetworkClient _networkClient;
    protected CollisionFloor _collisionFloor;

    protected virtual void Main()
    {
        _gfxReplayPlayer = gameObject.AddComponent<GfxReplayPlayer>();
        _networkClient = gameObject.AddComponent<NetworkClient>();
        _collisionFloor = gameObject.AddComponent<CollisionFloor>();

        var replayFileLoader = gameObject.GetComponent<ReplayFileLoader>();
        if (replayFileLoader.enabled)
        {
            Debug.LogWarning("Replay file loader enabled. Disabling network client.");
            _networkClient.enabled = false;
        }
    }

    void Awake()
    {
        Main();
    }
}
