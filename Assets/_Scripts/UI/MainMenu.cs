using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        // Сбрасываем статический флаг двери
        NextLevelDoor.IsActivatedForCurrentLevel = false;

        // Уничтожаем старые объекты, которые не должны сохраняться
        GameObject gameManager = GameObject.Find("GameManager");
        if (gameManager != null) Destroy(gameManager);

        GameObject levelGenerator = GameObject.Find("LevelGenerator");
        if (levelGenerator != null) Destroy(levelGenerator);

        GameObject adaptiveDifficulty = GameObject.Find("AdaptiveDifficulty");
        if (adaptiveDifficulty != null) Destroy(adaptiveDifficulty);

        GameObject roomLoader = GameObject.Find("RoomLoader");
        if (roomLoader != null) Destroy(roomLoader);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) Destroy(player);

        // Загружаем сцену игры заново
        SceneManager.LoadScene("v.2.5");
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}