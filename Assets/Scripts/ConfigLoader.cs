/// <summary>
/// Loads a configuration from file.
/// Config defaults are used directly when running in the Editor. On device, they are used to populate config.txt at Android/data/com.meta.siro_hitl_vr_client/files/.
/// This file persists and can be edited between runs, e.g. by connecting via USB to a laptop.
/// </summary>
public class ConfigLoader
{
    [System.Serializable]
    public class Config
    {
        public string[] serverLocations;
    }

    private Config _config;
    private string[] _serverLocations;

    public ConfigLoader(string[] serverLocations)
    {
        _serverLocations = serverLocations;
    }

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
            serverLocations = _serverLocations,
        };
    }
}
