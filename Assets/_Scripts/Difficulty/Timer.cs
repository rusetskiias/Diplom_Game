using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    private float currentTime;
    private bool isRunning = true;
    private float lastLogTime;

    void Start()
    {
        currentTime = 0f;
        isRunning = true;
        lastLogTime = 0f;
    }

    void Update()
    {
        if (isRunning)
        {
            currentTime += Time.deltaTime;
            UpdateDisplay();

            //  аждые 5 секунд выводим врем€ в консоль
            if (currentTime >= lastLogTime + 5f)
            {
                lastLogTime = currentTime;
                int minutes = Mathf.FloorToInt(currentTime / 60);
                int seconds = Mathf.FloorToInt(currentTime % 60);
            }
        }
    }

    void UpdateDisplay()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60);
            int seconds = Mathf.FloorToInt(currentTime % 60);
            timerText.text = $"Time: {minutes:00}:{seconds:00}";
        }
    }

    public float StopAndSave()
    {
        isRunning = false;
        return currentTime;
    }

    public void StartTimer()
    {
        isRunning = true;
    }

    public void ResetTimer()
    {
        currentTime = 0f;
        lastLogTime = 0f;
        isRunning = true;
    }

    public float GetCurrentTime()
    {
        return currentTime;
    }
}