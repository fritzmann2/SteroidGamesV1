using UnityEngine;
using UnityEditor;
using System.Reflection;

[InitializeOnLoad]
public class EditorMasterZoom : EditorWindow
{
    // Speichert den Zoom-Faktor (Standard: 1.25 = 125%)
    private static float zoomFactor
    {
        get { return EditorPrefs.GetFloat("EditorMasterZoom", 1.25f); }
        set { EditorPrefs.SetFloat("EditorMasterZoom", value); }
    }

    static EditorMasterZoom()
    {
        // Wendet den Zoom kurz nach dem Start automatisch an
        EditorApplication.delayCall += ApplyZoom;
    }

    [MenuItem("Window/Editor Master Zoom")]
    public static void ShowWindow()
    {
        GetWindow<EditorMasterZoom>("Zoom");
    }

    void OnGUI()
    {
        GUILayout.Label("Unity Retina DPI Simulator", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox("Dieser Ansatz ändert KEINE Schriften. Er zwingt Unitys interne Rendering-Engine dazu, das UI hochzuskalieren (wie bei der fehlenden Windows-Einstellung).", MessageType.Info);

        zoomFactor = EditorGUILayout.Slider("Zoom-Faktor", zoomFactor, 1.0f, 2.5f);

        if (GUILayout.Button("Zoom Anwenden"))
        {
            ApplyZoom();
            ShowNotification(new GUIContent("Zoom aktiv! Ggf. Tab neu öffnen."));
        }
    }

    private static void ApplyZoom()
    {
        // Tiefes Reflection in Unitys Core-Engine, um den globalen DPI-Wert zu überschreiben
        var type = typeof(GUIUtility);
        
        // Suche das interne Property für die DPI-Skalierung
        var prop = type.GetProperty("pixelsPerPoint", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        
        if (prop != null)
        {
            try
            {
                prop.SetValue(null, zoomFactor, null);
            }
            catch
            {
                // Fallback: Wenn das Property geschützt ist, greifen wir direkt auf die interne Variable zu
                var field = type.GetField("s_PixelsPerPoint", BindingFlags.Static | BindingFlags.NonPublic);
                if (field != null)
                {
                    field.SetValue(null, zoomFactor);
                }
            }
        }

        // Zwinge alle Fenster im Editor, sich sofort mit dem neuen Zoom neu zu zeichnen
        foreach (EditorWindow window in Resources.FindObjectsOfTypeAll<EditorWindow>())
        {
            window.Repaint();
        }
    }
}