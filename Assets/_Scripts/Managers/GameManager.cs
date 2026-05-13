using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

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

    public void StartNewRun()
    {
        Debug.Log("Новый забег начат");
    }

    public void NextLevel()
    {
        Debug.Log("Переход на следующий уровень");
    }

    public void GameOver()
    {
        Debug.Log("Игра окончена");
    }
}