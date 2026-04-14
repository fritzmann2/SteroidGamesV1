using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    
    public GameControls Controls { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
            
            Controls = new GameControls();
            
            LoadSavedControls();
            
            Controls.Enable();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadSavedControls()
    {
        Controls.asset.RemoveAllBindingOverrides(); 
        string savedRebinds = PlayerPrefs.GetString("CustomControls", string.Empty);
        if (!string.IsNullOrEmpty(savedRebinds))
        {
            Controls.asset.LoadBindingOverridesFromJson(savedRebinds);
        }
    }
}