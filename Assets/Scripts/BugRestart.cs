using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class BugRestart : MonoBehaviour
{
    private static BugRestart instance;

    private void Awake()
    {
        // Singleton kontrolü
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Sahneler arasýnda taþýnýr
        }
        else
        {
            Destroy(gameObject); // Fazlalýk olanlarý yok et
            return;
        }
    }

    private void Update()
    {
        // Shift + R kombinasyonunu kontrol et
        if (Keyboard.current[Key.LeftShift].isPressed && Keyboard.current[Key.R].wasPressedThisFrame)
        {
            ReloadCurrentScene();
        }
    }

    private void ReloadCurrentScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
        Debug.Log("Sahne yeniden yüklendi: " + currentSceneIndex);
    }
}
