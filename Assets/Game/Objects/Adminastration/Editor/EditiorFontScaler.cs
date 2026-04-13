using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class EditorFontScaler : EditorWindow
{
    private static bool isStartupApplied = false;

    private static int customFontSize
    {
        get { return EditorPrefs.GetInt("CustomEditorFontSize", 18); }
        set { EditorPrefs.SetInt("CustomEditorFontSize", value); }
    }

    static EditorFontScaler()
    {
        EditorApplication.hierarchyWindowItemOnGUI += TryApplyOnStartup;
        EditorApplication.projectWindowItemOnGUI += TryApplyOnStartup;
    }

    private static void TryApplyOnStartup(int instanceID, Rect selectionRect) { TryApply(); }
    private static void TryApplyOnStartup(string guid, Rect selectionRect) { TryApply(); }

    private static void TryApply()
    {
        if (!isStartupApplied && Event.current != null)
        {
            ApplyScale();
            isStartupApplied = true;
            
            EditorApplication.hierarchyWindowItemOnGUI -= TryApplyOnStartup;
            EditorApplication.projectWindowItemOnGUI -= TryApplyOnStartup;
        }
    }

    [MenuItem("Window/Editor Font Scaler")]
    public static void ShowWindow()
    {
        GetWindow<EditorFontScaler>("Font Scaler");
    }

    void OnGUI()
    {
        GUILayout.Label("Unity Linux Font Fix V5", EditorStyles.boldLabel);
        customFontSize = EditorGUILayout.IntSlider("Font Size", customFontSize, 10, 36);

        if (GUILayout.Button("Apply Size"))
        {
            ApplyScale();
            ShowNotification(new GUIContent("Angewendet! Tab neu öffnen."));
        }
    }

    private static void ApplyScale()
    {
        if (Event.current == null) return; 

        EditorWindow[] allWindows = Resources.FindObjectsOfTypeAll<EditorWindow>();
        foreach (EditorWindow window in allWindows)
        {
            if (window.rootVisualElement != null)
            {
                window.rootVisualElement.style.fontSize = customFontSize;
            }
            window.Repaint();
        }

        GUI.skin.label.fontSize = customFontSize;
        GUI.skin.button.fontSize = customFontSize;
        GUI.skin.textField.fontSize = customFontSize;
        GUI.skin.box.fontSize = customFontSize;

        if (GUI.skin.customStyles != null)
        {
            foreach (GUIStyle style in GUI.skin.customStyles)
            {
                style.fontSize = customFontSize;
                
                if (style.fixedHeight > 0 && style.fixedHeight < customFontSize + 8)
                {
                    style.fixedHeight = customFontSize + 8;
                }
            }
        }

        string[] hardcodedStyles = { "TV Line", "PR Label", "ProjectBrowserGridLabel" };
        foreach (string s in hardcodedStyles)
        {
            GUIStyle style = GUI.skin.FindStyle(s) ?? EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle(s);
            if (style != null) style.fixedHeight = customFontSize + 8;
        }
    }
}