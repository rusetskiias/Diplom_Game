using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        Debug.Log("Здоровье обновлено: " + currentHealth + " / " + maxHealth);
    }

    public void ShowGameOver()
    {
        Debug.Log("Показать экран Game Over");
    }
}