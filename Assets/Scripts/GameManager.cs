using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Singleton referansý

    [Header("UI Referanslarý")]
    public GameObject gameOverUI; // GameOver UI referansý
    public GameObject startScreenUI; // StartScreen UI referansý

    [Header("Müzik Ayarlarý")]
    public AudioSource introMusic; // Açýlýþ müziði
    public AudioSource gameMusic; // Oyun müziði (opsiyonel)
    public float fadeOutDuration = 2f; // Müzik geçiþ süresi

    private bool gameStarted = false;
    private float musicTimer = 0f;
    private float originalTimeScale;

    private void Awake()
    {
        // Singleton kontrolü (sadece sahne içinde)
        if (Instance == null || Instance == this)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Ayný sahnede birden fazla GameManager varsa yok et
            return;
        }

        // Orijinal Time Scale'i kaydet
        originalTimeScale = Time.timeScale;
    }

    private void Start()
    {
        // Oyun baþlangýcýný ayarla
        InitializeGame();
    }

    //private void Update()
    //{
    //    // Eðer oyun henüz baþlamadýysa ve intro müziði varsa
    //    if (!gameStarted && introMusic != null)
    //    {
    //        // Intro müziði bittiyse veya kullanýcý bir tuþa bastýysa
    //        if (!introMusic.isPlaying || Input.anyKeyDown)
    //        {
    //            StartGame();
    //        }

    //        // Debug için müzik zamaný
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
            Debug.Log($"GameMusic ses seviyesi güncellendi: {newVolume}");
        }
    }

    private void InitializeGame()
    {
        // Zaman ölçeðini durdur
        Time.timeScale = 0f;

        // Tüm animasyonlarý durdur
        Animator[] animators = FindObjectsOfType<Animator>();
        foreach (var animator in animators)
        {
            animator.enabled = false;
        }

        // StartScreen UI'yi göster
        if (startScreenUI != null)
        {
            startScreenUI.SetActive(true);
        }
        else
        {
            Debug.LogWarning("StartScreen UI referansý atanmadý!");
        }

        // GameOver UI'yi gizle
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }

        // Intro müziðini baþlat
        if (introMusic != null)
        {
            introMusic.Play();
        }
        else
        {
            Debug.LogWarning("Intro müziði bulunamadý!");
        }

        // 2 saniye bekle ve oyunu baþlat
        StartCoroutine(StartGameAfterDelay(2f));

        gameStarted = false;
    }

    // 2 saniye bekledikten sonra oyunu baþlatan coroutine
    private System.Collections.IEnumerator StartGameAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay); // Gerçek zamanlý bekleme
        StartGame();
    }
    public void StartGame()
    {
        if (gameStarted) return; // Zaten baþladýysa çýkýþ yap

        // Intro müziðini durdur veya kademeli olarak kapat
        if (introMusic != null && introMusic.isPlaying)
        {
            StartCoroutine(FadeOutMusic(introMusic, fadeOutDuration));
        }

        // StartScreen UI'yi gizle
        if (startScreenUI != null)
        {
            startScreenUI.SetActive(false);
        }

        // Zaman ölçeðini normal hýza getir
        Time.timeScale = originalTimeScale;

        // Tüm animasyonlarý yeniden baþlat
        Animator[] animators = FindObjectsOfType<Animator>();
        foreach (var animator in animators)
        {
            animator.enabled = true;
        }

        // Oyun müziðini baþlat (opsiyonel)
        if (gameMusic != null && !gameMusic.isPlaying)
        {
            gameMusic.Play();
        }

        gameStarted = true;
        Debug.Log("Oyun baþladý!");
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
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Müziði kademeli olarak kapatma
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
