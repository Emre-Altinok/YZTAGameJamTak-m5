using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class BugRestart : MonoBehaviour
{
    private static BugRestart instance;

    private void Awake()
    {
        // Singleton kontrol�
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Sahneler aras�nda ta��n�r
        }
        else
        {
            Destroy(gameObject); // Fazlal�k olanlar� yok et
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
        Debug.Log("Sahne yeniden y�klendi: " + currentSceneIndex);
    }
}
