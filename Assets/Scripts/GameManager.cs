using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Singleton referans�

    [Header("UI Referanslar�")]
    public GameObject gameOverUI; // GameOver UI referans�
    public GameObject startScreenUI; // StartScreen UI referans�

    [Header("M�zik Ayarlar�")]
    public AudioSource introMusic; // A��l�� m�zi�i
    public AudioSource gameMusic; // Oyun m�zi�i (opsiyonel)
    public float fadeOutDuration = 2f; // M�zik ge�i� s�resi

    private bool gameStarted = false;
    private float musicTimer = 0f;
    private float originalTimeScale;

    private void Awake()
    {
        // Singleton kontrol� (sadece sahne i�inde)
        if (Instance == null || Instance == this)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Ayn� sahnede birden fazla GameManager varsa yok et
            return;
        }

        // Orijinal Time Scale'i kaydet
        originalTimeScale = Time.timeScale;
    }

    private void Start()
    {
        // Oyun ba�lang�c�n� ayarla
        InitializeGame();
    }

    //private void Update()
    //{
    //    // E�er oyun hen�z ba�lamad�ysa ve intro m�zi�i varsa
    //    if (!gameStarted && introMusic != null)
    //    {
    //        // Intro m�zi�i bittiyse veya kullan�c� bir tu�a bast�ysa
    //        if (!introMusic.isPlaying || Input.anyKeyDown)
    //        {
    //            StartGame();
    //        }

    //        // Debug i�in m�zik zaman�
    //        musicTimer = introMusic.time;
    //    }
    //}

    private void OnEnable()
    {
        UIManager.OnMusicVolumeChanged += UpdateMusicVolume;
    }

    private void OnDisable()
    {
        UIManager.OnMusicVolumeChanged -= UpdateMusicVolume;
    }

    private void UpdateMusicVolume(float newVolume)
    {
        if (gameMusic != null)
        {
            gameMusic.volume = newVolume;
            Debug.Log($"GameMusic ses seviyesi g�ncellendi: {newVolume}");
        }
    }

    private void InitializeGame()
    {
        // Zaman �l�e�ini durdur
        Time.timeScale = 0f;

        // T�m animasyonlar� durdur
        Animator[] animators = FindObjectsOfType<Animator>();
        foreach (var animator in animators)
        {
            animator.enabled = false;
        }

        // StartScreen UI'yi g�ster
        if (startScreenUI != null)
        {
            startScreenUI.SetActive(true);
        }
        else
        {
            Debug.LogWarning("StartScreen UI referans� atanmad�!");
        }

        // GameOver UI'yi gizle
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }

        // Intro m�zi�ini ba�lat
        if (introMusic != null)
        {
            introMusic.Play();
        }
        else
        {
            Debug.LogWarning("Intro m�zi�i bulunamad�!");
        }

        // 2 saniye bekle ve oyunu ba�lat
        StartCoroutine(StartGameAfterDelay(2f));

        gameStarted = false;
    }

    // 2 saniye bekledikten sonra oyunu ba�latan coroutine
    private System.Collections.IEnumerator StartGameAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay); // Ger�ek zamanl� bekleme
        StartGame();
    }
    public void StartGame()
    {
        if (gameStarted) return; // Zaten ba�lad�ysa ��k�� yap

        // Intro m�zi�ini durdur veya kademeli olarak kapat
        if (introMusic != null && introMusic.isPlaying)
        {
            StartCoroutine(FadeOutMusic(introMusic, fadeOutDuration));
        }

        // StartScreen UI'yi gizle
        if (startScreenUI != null)
        {
            startScreenUI.SetActive(false);
        }

        // Zaman �l�e�ini normal h�za getir
        Time.timeScale = originalTimeScale;

        // T�m animasyonlar� yeniden ba�lat
        Animator[] animators = FindObjectsOfType<Animator>();
        foreach (var animator in animators)
        {
            animator.enabled = true;
        }

        // Oyun m�zi�ini ba�lat (opsiyonel)
        if (gameMusic != null && !gameMusic.isPlaying)
        {
            gameMusic.Play();
        }

        gameStarted = true;
        Debug.Log("Oyun ba�lad�!");
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
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // M�zi�i kademeli olarak kapatma
    private System.Collections.IEnumerator FadeOutMusic(AudioSource audioSource, float duration)
    {
        float startVolume = audioSource.volume;

        for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t / duration);
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;
    }
}
