using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Singleton referans�
    public GameObject gameOverUI; // GameOver UI referans�

    private void Awake()
    {
        // Singleton kontrol�
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowGameOverUI()
    {
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true); // GameOver UI'yi aktif et
        }
        else
        {
            Debug.LogError("GameOver UI referans� atanmad�!");
        }
    }

    public void Restart()
    {
        // Oyunu yeniden ba�lat
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }


}
