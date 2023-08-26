using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Transform), true)]
[CanEditMultipleObjects]
public class RotationQuaternionInspector : Editor
{
    private SerializedProperty m_Rotation;
    private Vector4 _stagingQuaternionValues;
    private Vector3 _preRotationEuler = Vector3.zero;
    private Vector3 _postRotationEuler = Vector3.zero;

    void OnEnable()
    {
        // Fetch the serialized property for rotation
        this.m_Rotation = this.serializedObject.FindProperty("m_LocalRotation");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Draw the default inspector, but skip the rotation property
        DrawPropertiesExcluding(serializedObject, "m_LocalRotation");

        // Get the current quaternion values
        Quaternion rotation = this.m_Rotation.quaternionValue;
        Vector4 currentQuaternionValues = new Vector4(rotation.x, rotation.y, rotation.z, rotation.w);

        // Display the current rotation as Quaternion
        EditorGUILayout.Vector4Field("Current Quaternion (X, Y, Z, W)", currentQuaternionValues);

        // Create a field for the staging Quaternion values
        _stagingQuaternionValues = EditorGUILayout.Vector4Field("Staging Quaternion (X, Y, Z, W)", _stagingQuaternionValues);

        // Pre-Rotation and Post-Rotation fields
        _preRotationEuler = EditorGUILayout.Vector3Field("Pre-Rotation Euler", _preRotationEuler);
        _postRotationEuler = EditorGUILayout.Vector3Field("Post-Rotation Euler", _postRotationEuler);

        // If the Apply Quaternion button is pressed, apply pre, staged, and post rotations and normalize the result
        if (GUILayout.Button("Apply Quaternion"))
        {
            Quaternion preRotation = Quaternion.Euler(_preRotationEuler);
            Quaternion stagedRotation = new Quaternion(_stagingQuaternionValues.x, _stagingQuaternionValues.y, _stagingQuaternionValues.z, _stagingQuaternionValues.w);
            Quaternion postRotation = Quaternion.Euler(_postRotationEuler);

            Quaternion finalRotation = postRotation * stagedRotation * preRotation;

            m_Rotation.quaternionValue = finalRotation.normalized;
        }

        // Show Euler angles
        Vector3 euler = rotation.eulerAngles;
        EditorGUI.BeginChangeCheck();
        euler = EditorGUILayout.Vector3Field("Euler Angles", euler);
        if (EditorGUI.EndChangeCheck())
        {
            m_Rotation.quaternionValue = Quaternion.Euler(euler);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
