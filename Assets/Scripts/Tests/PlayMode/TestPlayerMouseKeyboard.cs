using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class TestPlayerMouseKeyboard
{
    private const string SCENE_PATH = "Assets/Scenes/PlayerMouseKeyboard.unity";

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        var sceneParams = new LoadSceneParameters();
        sceneParams.loadSceneMode = LoadSceneMode.Single;
        yield return EditorSceneManager.LoadSceneAsyncInPlayMode(SCENE_PATH, sceneParams);
    }

    [UnityTest]
    public IEnumerator TestSceneContent()
    {
        // Test initial setup.
        {
            var objs = GameObject.FindObjectsByType<AppMouseKeyboard>(FindObjectsSortMode.None);
            Assert.AreEqual(objs.Length, 1);
        }
        {
            var objs = GameObject.FindObjectsByType<Camera>(FindObjectsSortMode.None);
            Assert.AreEqual(objs.Length, 1);
            Camera camera = objs[0];
            Assert.AreEqual(camera.tag, "MainCamera");
        }
        {
            var objs = GameObject.FindObjectsByType<Light>(FindObjectsSortMode.None);
            Assert.AreNotEqual(objs.Length, 0);
        }
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator Teardown()
    {
        yield return null;
    }
}
