using UnityEngine;
using System.IO;
using UnityEngine.Assertions;

public class ConfigLoader : MonoBehaviour
{
    [System.Serializable]
    public class Config
    {
        public string[] serverLocations;
        // public int visualQuality;
    }

    [Header("Config Defaults")]
    [Tooltip("Config defaults are used directly when running in the Editor. On device, they are used to populate config.txt at Android/data/com.meta.siro_hitl_vr_client/files/. This file persists and can be edited between runs, e.g. by connecting via USB to a laptop.")]
    [SerializeField]
    private bool _mouseoverForTooltip;  // dummy member so we can add tooltip in Inspector pane
    [Space(10)] // Add a little spacing for clarity

    [SerializeField] private string[] defaultServerLocations = { "1.2.3.4", "1.2.3.5:6789" };
    // [SerializeField] private int defaultVisualQuality = 2;

    private Config _config;

    public Config AppConfig
    {
        get
        {
            if (_config == null)
            {
                LoadOrCreateConfig();
            }
            return _config;
        }
    }

    private void LoadOrCreateConfig()
    {
#if UNITY_EDITOR
        _config = CreateDefaultConfig();
        return;
#else

        string configPath = Path.Combine(Application.persistentDataPath, "config.txt");
        if (File.Exists(configPath))
        {
            string json = File.ReadAllText(configPath);
            _config = JsonUtility.FromJson<Config>(json);
        }
        else
        {
            _config = CreateDefaultConfig();
            string json = JsonUtility.ToJson(_config);
            File.WriteAllText(configPath, json);
        }
#endif
    }

    private Config CreateDefaultConfig()
    {
        return new Config
        {
            serverLocations = defaultServerLocations,
            // visualQuality = defaultVisualQuality
        };
    }
}
