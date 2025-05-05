using UnityEngine;
using UnityEngine.SceneManagement;

public class SkipShortcut : MonoBehaviour
{
    void Update()
    {
        // Eðer Enter tuþuna basýlýrsa
        if (Input.GetKeyDown(KeyCode.Return)) // Return, Enter tuþunu temsil eder
        {
            // Mevcut sahnenin index'ini al
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

            // Bir sonraki sahneyi yükle
            SceneManager.LoadScene(currentSceneIndex + 1);
        }
    }
}
