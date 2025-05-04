using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Singleton referansý
    public GameObject gameOverUI; // GameOver UI referansý

    private void Awake()
    {
        // Singleton kontrolü
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
            Debug.LogError("GameOver UI referansý atanmadý!");
        }
    }

    public void Restart()
    {
        // Oyunu yeniden baþlat
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }


}
