
using UnityEngine;

/// <summary>
/// Creates a large planar collider at Y=0 to prevent rigid bodies from falling through the scene.
/// </summary>
public class CollisionFloor : MonoBehaviour
{
    private GameObject _floorObject;

    void Awake()
    {
        _floorObject = new GameObject("Collision Floor");
        _floorObject.transform.position = Vector3.zero;
        _floorObject.transform.rotation = Quaternion.identity;
        _floorObject.transform.localScale = Vector3.one;
        var collider = _floorObject.AddComponent<BoxCollider>();
        collider.center = new Vector3(0.0f, -1.0f, 0.0f);
        collider.size = new Vector3(1000.0f, 2.0f, 1000.0f);
    }
}