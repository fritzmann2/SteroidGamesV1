using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class ButtonThemeManager : MonoBehaviour
{
    [Header("Design-Einstellungen")]
    public Color normalColor = Color.white;
    public Color highlightedColor = new Color32(0x6D, 0x6D, 0x6D, 0xFF);
    public Color pressedColor = new Color32(0x2B, 0x2B, 0x2B, 0xFF);
    public Color selectedColor = new Color32(0x6D, 0x6D, 0x6D, 0xFF);
    
    [Range(1f, 5f)]
    public float colorMultiplier = 2f;

    [ContextMenu("Farben auf ALLE Buttons anwenden (Szene & Projekt)!")]
    public void ApplyThemeToAllButtons()
    {
#if UNITY_EDITOR
        int sceneCount = ApplyToSceneButtons();
        int prefabCount = ApplyToPrefabAssets();

        Debug.Log($"Erfolg! Design angewendet auf: {sceneCount} Szene-Buttons und {prefabCount} Prefab-Dateien.");
#endif
    }

#if UNITY_EDITOR
    private int ApplyToSceneButtons()
    {
        Button[] allButtons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Button btn in allButtons)
        {
            UpdateColors(btn);
            EditorUtility.SetDirty(btn);
        }
        return allButtons.Length;
    }

    private int ApplyToPrefabAssets()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        int updatedPrefabs = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            
            if (!path.StartsWith("Assets/"))
            {
                continue; 
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            Button[] btns = prefab.GetComponentsInChildren<Button>(true);

            if (btns.Length > 0)
            {
                foreach (Button btn in btns)
                {
                    UpdateColors(btn);
                }

                EditorUtility.SetDirty(prefab);
                PrefabUtility.SavePrefabAsset(prefab);
                updatedPrefabs++;
            }
        }
        AssetDatabase.SaveAssets();
        return updatedPrefabs;
    }

    private void UpdateColors(Button btn)
    {
        ColorBlock cb = btn.colors;
        cb.normalColor = normalColor;
        cb.highlightedColor = highlightedColor;
        cb.pressedColor = pressedColor;
        cb.selectedColor = selectedColor;
        cb.colorMultiplier = colorMultiplier;
        btn.colors = cb;
    }
#endif
}