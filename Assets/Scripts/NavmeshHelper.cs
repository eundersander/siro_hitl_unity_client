using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// This class should be provided a navmesh as triangles (UpdateNavmesh).
/// It currently uses the navmesh to restrict VR teleportation, but we don't
/// fully restrict avatar movement. Because of this, user code should still
/// provide a floor collision plane, to prevent the avatar falling through
/// the world when it leaves the navmesh.
/// </summary>
public class NavmeshHelper : MonoBehaviour
{
    private TeleportationArea _teleportationArea;
    private MeshCollider _meshCollider;

    private static Vector3[] CreateGroundPlane(float size)
    {
        // Each three indices represent one triangle
        Vector3[] vertices = new Vector3[6];

        float halfSize = size / 2;

        // First triangle vertices (bottom left triangle)
        vertices[0] = new Vector3(-halfSize, 0, -halfSize); // Bottom left corner
        vertices[1] = new Vector3(halfSize, 0, -halfSize);  // Bottom right corner
        vertices[2] = new Vector3(-halfSize, 0, halfSize);  // Top left corner

        // Second triangle vertices (top right triangle)
        vertices[3] = new Vector3(halfSize, 0, -halfSize);  // Bottom right corner
        vertices[4] = new Vector3(halfSize, 0, halfSize);   // Top right corner
        vertices[5] = new Vector3(-halfSize, 0, halfSize);  // Top left corner

        return vertices;
    }

    public void UpdateNavmesh(Vector3[] vertices, bool doDoublesided)
    {
        if (vertices == null || vertices.Length % 3 != 0)
        {
            Debug.LogError("Vertices array is invalid. It must contain a multiple of three vertices.");
            return;
        }

        // Create a new mesh
        Mesh mesh = new Mesh();
        mesh.name = "navmesh";

        // Set the vertices
        mesh.vertices = vertices;

        // Define triangles based on the vertices
        int[] triangles;
        if (doDoublesided)
        {
            // Double the triangle array size for the two-sided mesh
            triangles = new int[vertices.Length * 2];
            for (int i = 0; i < vertices.Length; i += 3)
            {
                // Front-facing triangle
                triangles[i * 2] = i;
                triangles[i * 2 + 1] = i + 1;
                triangles[i * 2 + 2] = i + 2;
                // Back-facing triangle (reversing the order for the winding)
                triangles[i * 2 + 3] = i;
                triangles[i * 2 + 4] = i + 2;
                triangles[i * 2 + 5] = i + 1;
            }
        }
        else
        {
            triangles = new int[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                triangles[i] = i;
            }
        }
        mesh.triangles = triangles;

        // Calculate normals (required for collision)
        mesh.RecalculateNormals();

        // Assign the mesh to the Mesh Collider
        _meshCollider.sharedMesh = mesh;
    }


    void Start()
    {
        // todo: instead of requiring these components, this class should
        // programmatically create them. See
        // https://app.asana.com/0/1205278804336681/1205931235223778
        _teleportationArea = GetComponent<TeleportationArea>();
        Assert.IsTrue(_teleportationArea);  // our object should have a TeleportationArea

        _meshCollider = GetComponent<MeshCollider>();
        Assert.IsTrue(_meshCollider);
        Assert.IsTrue(_teleportationArea.colliders.Count == 1);

        // By default, we use a ground plane as the navmesh. This allows
        // basically unrestricted teleportation.
        UpdateNavmesh(CreateGroundPlane(200), true);
    }

}

