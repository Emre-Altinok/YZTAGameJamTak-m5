using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject settingsPanel;

    public void StartGame()
    {

        SceneManager.LoadScene(1);
        Debug.Log("Oyun ba�lat�ld�!");
    }

    public void OpenSettings()
    {
        // Ayarlar panelini g�ster
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
        Debug.Log("Ayarlar men�s� a��ld�!");
    }
    public void CloseSettings()
    {
        // Ayarlar panelini g�ster
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        Debug.Log("Ayarlar men�s� kapand�!");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        // Editor modunda oynatmay� durdur
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Ger�ek uygulamada oyundan ��k
        Application.Quit();
#endif
        Debug.Log("Oyundan ��k�l�yor...");
    }
}
