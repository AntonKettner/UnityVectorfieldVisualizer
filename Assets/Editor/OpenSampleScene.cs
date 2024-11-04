using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement; // Needed for EditorSceneManager
using UnityEngine.SceneManagement; // Needed for SceneManager

public static class OpenSampleScene
{
    [InitializeOnLoadMethod]
    static void OpenScene()
    {
        EditorApplication.update += LoadScene;
    }

    static void LoadScene()
    {
        EditorApplication.update -= LoadScene;
        string defaultScenePath = "Assets/Scenes/SampleScene.unity"; // change to your scene path

        // Remove the Application check as Application is mainly used in runtime, here we're in editor context
        Scene scene = SceneManager.GetSceneByPath(defaultScenePath);
        if (!scene.isLoaded)
        {
            EditorSceneManager.OpenScene(defaultScenePath);
        }
    }
}