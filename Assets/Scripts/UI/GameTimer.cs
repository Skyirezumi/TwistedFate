using UnityEngine;
using TMPro;
using System;

public class GameTimer : MonoBehaviour
{
    [Header("Timer Display")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private bool showMilliseconds = false;
    
    [Header("Timer Settings")]
    [SerializeField] private bool startOnAwake = true;
    [SerializeField] private bool countUp = true;
    
    private float elapsedTime = 0f;
    private bool isRunning = false;
    
    private void Awake()
    {
        if (startOnAwake)
        {
            StartTimer();
        }
    }
    
    private void Update()
    {
        if (isRunning)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimerDisplay();
        }
    }
    
    public void StartTimer()
    {
        isRunning = true;
    }
    
    public void StopTimer()
    {
        isRunning = false;
    }
    
    public void ResetTimer()
    {
        elapsedTime = 0f;
        UpdateTimerDisplay();
    }
    
    public float GetElapsedTime()
    {
        return elapsedTime;
    }
    
    private void UpdateTimerDisplay()
    {
        if (timerText == null) return;
        
        TimeSpan timeSpan = TimeSpan.FromSeconds(elapsedTime);
        
        if (showMilliseconds)
        {
            timerText.text = string.Format("{0:00}:{1:00}.{2:00}", 
                timeSpan.Minutes, 
                timeSpan.Seconds,
                timeSpan.Milliseconds / 10);
        }
        else
        {
            timerText.text = string.Format("{0:00}:{1:00}", 
                timeSpan.Minutes, 
                timeSpan.Seconds);
        }
    }
} 