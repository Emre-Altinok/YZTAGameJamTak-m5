using UnityEngine;
using UnityEngine.SceneManagement;

public class SkipShortcut : MonoBehaviour
{
    void Update()
    {
        // E�er Enter tu�una bas�l�rsa
        if (Input.GetKeyDown(KeyCode.Return)) // Return, Enter tu�unu temsil eder
        {
            // Mevcut sahnenin index'ini al
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

            // Bir sonraki sahneyi y�kle
            SceneManager.LoadScene(currentSceneIndex + 1);
        }
    }
}
