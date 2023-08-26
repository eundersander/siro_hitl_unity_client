using UnityEngine;
using UnityEditor;
using System.IO;

public class UpdateDataFolderWindow : EditorWindow
{
    private string externalDataPath = "";  // Path where new data is generated
    private static string internalDataPath = "Assets/Resources/data/";

    [MenuItem("Tools/Update Data Folder...")]
    static void Init()
    {
        UpdateDataFolderWindow window = (UpdateDataFolderWindow)EditorWindow.GetWindow(typeof(UpdateDataFolderWindow));
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Update Data Folder Settings", EditorStyles.boldLabel);

        externalDataPath = EditorGUILayout.TextField("External Data Path:", externalDataPath);

        if (GUILayout.Button("Update Data Folder"))
        {
            UpdateData();
        }
    }

    private int fileCount = 0; // To keep track of how many files have been copied

    void UpdateData()
    {
        // Reset file count for new operation
        fileCount = 0;

        // Delete old data files
        if (Directory.Exists(internalDataPath))
        {
            Directory.Delete(internalDataPath, true);
            File.Delete(internalDataPath + ".meta"); // Also delete the associated meta file
        }

        // Copy new data files and directories
        if (Directory.Exists(externalDataPath))
        {
            RecursiveCopy(new DirectoryInfo(externalDataPath), new DirectoryInfo(internalDataPath));
            Debug.Log($"Copied {fileCount} files (finished).");
        }
        else
        {
            Debug.LogError("External data path not found: " + externalDataPath);
            return;
        }

        // Refresh the Unity asset database to recognize the changes
        AssetDatabase.Refresh();
    }

    void RecursiveCopy(DirectoryInfo sourceDir, DirectoryInfo targetDir)
    {
        Directory.CreateDirectory(targetDir.FullName);

        // Copy each file into the new directory
        foreach (FileInfo file in sourceDir.GetFiles())
        {
            file.CopyTo(Path.Combine(targetDir.FullName, file.Name), true);
            fileCount++;

            // Print progress for every 10 files copied
            if (fileCount % 10 == 0)
            {
                Debug.Log($"Copied {fileCount} files.");
            }
        }

        // Copy each subdirectory using recursion
        foreach (DirectoryInfo subDir in sourceDir.GetDirectories())
        {
            DirectoryInfo nextTargetSubDir = targetDir.CreateSubdirectory(subDir.Name);
            RecursiveCopy(subDir, nextTargetSubDir);
        }
    }

}
