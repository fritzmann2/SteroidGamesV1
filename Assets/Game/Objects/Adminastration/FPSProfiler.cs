using System;
using System.Collections.Generic;
using UnityEngine;

public class FPSProfiler : MonoBehaviour
{
    [Header("Settings")]
    public float updateInterval = 0.5f; 
    public int frameBufferLength = 1000; 
    private float currentFps;
    private float averageFps;
    private float onePercentLowFps;
    private float absoluteLowestFps = float.MaxValue;
    private float timeSinceLastUpdate = 0f;
    private int framesSinceLastUpdate = 0;
    
    private float totalTimeForAvg = 0f;
    private int totalFramesForAvg = 0;

    private Queue<float> frameTimes = new Queue<float>();
    private float[] frameTimeArray;

    private GameControls controls;
    private bool showStats = false; 

    void Start()
    {
        controls = InputManager.Instance.Controls;
        frameTimeArray = new float[frameBufferLength];
        Application.targetFrameRate = 200;
    }

    void Update()
    {
        if (controls.Gameplay.ShowFPS.triggered) 
        {
            showStats = !showStats; 
        }

        float dt = Time.unscaledDeltaTime;
        frameTimes.Enqueue(dt);
        if (frameTimes.Count > frameBufferLength)
        {
            frameTimes.Dequeue();
        }
        
        timeSinceLastUpdate += dt;
        framesSinceLastUpdate++;
        
        totalTimeForAvg += dt;
        totalFramesForAvg++;
        
        if (timeSinceLastUpdate >= updateInterval)
        {
            currentFps = framesSinceLastUpdate / timeSinceLastUpdate;
            averageFps = totalFramesForAvg / totalTimeForAvg;
            
            CalculateOnePercentLow();
            
            if (Time.timeSinceLevelLoad > 2f && currentFps < absoluteLowestFps)
            {
                absoluteLowestFps = currentFps;
            }
            timeSinceLastUpdate = 0f;
            framesSinceLastUpdate = 0;
        }
    }

    private void CalculateOnePercentLow()
    {
        int count = frameTimes.Count;
        if (count < 100) return; 
        frameTimes.CopyTo(frameTimeArray, 0);
        Array.Sort(frameTimeArray, 0, count);
        int onePercentCount = Mathf.Max(1, count / 100);
        float onePercentLowTime = frameTimeArray[count - onePercentCount];

        onePercentLowFps = 1.0f / onePercentLowTime;
    }

    void OnGUI()
    {
        if (!showStats) return;

        GUIStyle style = new GUIStyle();
        int w = Screen.width, h = Screen.height;
        Rect rect = new Rect(20, 20, w, h * 2 / 100);
        
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = Mathf.Clamp(h / 30, 20, 40); 
        
        if (currentFps < 30) style.normal.textColor = Color.red;
        else if (currentFps < 60) style.normal.textColor = Color.yellow;
        else style.normal.textColor = Color.green;

        string text = $"FPS: {Mathf.RoundToInt(currentFps)}\n" +
                      $"Avg: {Mathf.RoundToInt(averageFps)}\n" +
                      $"1% Low: {Mathf.RoundToInt(onePercentLowFps)}\n" +
                      $"Min: {Mathf.RoundToInt(absoluteLowestFps == float.MaxValue ? 0 : absoluteLowestFps)}";

        GUIStyle shadowStyle = new GUIStyle(style);
        shadowStyle.normal.textColor = Color.black;
        GUI.Label(new Rect(rect.x + 2, rect.y + 2, rect.width, rect.height), text, shadowStyle);
        
        GUI.Label(rect, text, style);
    }
}