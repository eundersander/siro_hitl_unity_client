using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class InputPrompt : EditorWindow
{
    public string InputValue = "";
    private Action<string> _callback;

    public static void ShowWindow(Action<string> callback)
    {
        InputPrompt window = (InputPrompt)EditorWindow.GetWindow(typeof(InputPrompt), true, "Specify Asset Type", true);
        window._callback = callback;
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Enter the name of the asset type you want to introspect:", EditorStyles.wordWrappedLabel);
        InputValue = EditorGUILayout.TextField("Asset Type:", InputValue);

        if (GUILayout.Button("OK"))
        {
            _callback.Invoke(InputValue);
            this.Close();
        }

        if (GUILayout.Button("Cancel"))
        {
            this.Close();
        }
    }
}


public class IntrospectAssets
{
    [MenuItem("Tools/Introspect All Asset Types in Resources")]
    private static void ListAssetsByType()
    {
        // Load all assets from the Resources folder
        UnityEngine.Object[] allAssets = Resources.LoadAll<UnityEngine.Object>("");

        Dictionary<string, int> assetTypeCounts = new Dictionary<string, int>();

        foreach (var asset in allAssets)
        {
            string typeName = asset.GetType().Name;

            if (assetTypeCounts.ContainsKey(typeName))
            {
                assetTypeCounts[typeName]++;
            }
            else
            {
                assetTypeCounts[typeName] = 1;
            }
        }

        Debug.Log($"Total asset types in Resources: {assetTypeCounts.Count}");

        foreach (var pair in assetTypeCounts)
        {
            Debug.Log($"Asset Type: {pair.Key}, Count: {pair.Value}");
        }
    }

    [MenuItem("Tools/Introspect Specific Asset Type in Resources")]
    private static void ListSpecificAssetType()
    {
        InputPrompt.ShowWindow(assetType =>
        {
            if (string.IsNullOrEmpty(assetType))
            {
                Debug.Log("No asset type specified.");
                return;
            }

            // Load all assets of the specified type from the Resources folder
            var allAssets = Resources.LoadAll("", System.Type.GetType($"UnityEngine.{assetType}, UnityEngine.CoreModule"));

            Debug.Log($"Total {assetType} assets in Resources: {allAssets.Length}");

            foreach (var asset in allAssets)
            {
                Debug.Log($"Asset Name: {asset.name}");
            }
        });
    }

}
