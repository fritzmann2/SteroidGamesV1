using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class BootSceneLoader
{
    // PASTE HIER DEN PFAD ZU DEINER SZENE:
    const string START_SCENE_PATH = "Assets/Scenes/StartMenu.unity"; 

    const string MENU_NAME = "Tools/Always Start From Menu";
    private static bool isEnabled;

    static BootSceneLoader()
    {
        isEnabled = EditorPrefs.GetBool(MENU_NAME, false);
        SetPlayModeStartScene(isEnabled);
    }

    [MenuItem(MENU_NAME)]
    private static void ToggleAction()
    {
        isEnabled = !isEnabled;
        EditorPrefs.SetBool(MENU_NAME, isEnabled);
        SetPlayModeStartScene(isEnabled);
    }

    [MenuItem(MENU_NAME, true)]
    private static bool ToggleActionValidate()
    {
        Menu.SetChecked(MENU_NAME, isEnabled);
        return true;
    }

    private static void SetPlayModeStartScene(bool enable)
    {
        if (enable)
        {
            var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(START_SCENE_PATH);
            if (scene != null) EditorSceneManager.playModeStartScene = scene;
            else UnityEngine.Debug.LogError($"Szene nicht gefunden: {START_SCENE_PATH}");
        }
        else
        {
            EditorSceneManager.playModeStartScene = null;
        }
    }
}