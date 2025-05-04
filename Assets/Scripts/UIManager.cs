using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject settingsPanel;

    public void StartGame()
    {

        SceneManager.LoadScene(1);
        Debug.Log("Oyun baþlatýldý!");
    }

    public void OpenSettings()
    {
        // Ayarlar panelini göster
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
        Debug.Log("Ayarlar menüsü açýldý!");
    }
    public void CloseSettings()
    {
        // Ayarlar panelini göster
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        Debug.Log("Ayarlar menüsü kapandý!");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        // Editor modunda oynatmayý durdur
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Gerçek uygulamada oyundan çýk
        Application.Quit();
#endif
        Debug.Log("Oyundan çýkýlýyor...");
    }
}
