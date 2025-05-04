using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject settingsPanel;

    [Header("UI Controls")]
    [SerializeField] private Slider musicVolumeSlider; // Müzik ses seviyesi slider'ý
    [SerializeField] private Slider sfxVolumeSlider; // Opsiyonel: Efekt ses seviyesi slider'ý

    [Header("Audio Settings")]
    [SerializeField] private AudioClip backgroundMusic; // Arkaplan müziði
    [SerializeField] private float musicVolume = 0.5f; // Varsayýlan ses düzeyi (0.0f - 1.0f)
    [SerializeField] private bool playMusicOnStart = true; // Baþlangýçta müzik çalsýn mý
    [SerializeField] private bool dontDestroyOnLoad = false; // Sahneler arasý geçiþlerde korunsun mu

    // Opsiyonel: Ses efektleri için ayrý AudioSource
    [SerializeField] private AudioSource sfxAudioSource;

    private AudioSource musicSource; // Müzik ses kaynaðý
    private static UIManager instance; // Singleton için

    // Singleton instance'ýna public eriþim
    public static UIManager Instance => instance;

    private void Awake()
    {
        // Singleton pattern - tek bir UIManager olmasýný saðlar
        if (instance == null)
        {
            instance = this;

            // Sahneler arasý geçiþlerde korunsun mu ayarý
            if (dontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Baþka bir UIManager varsa, bunu yok et
            Destroy(gameObject);
            return;
        }

        // Müzik kaynaðýný ayarla
        SetupAudioSource();
    }

    private void Start()
    {
        // Slider'larý baþlat
        InitializeSliders();

        // Eðer baþlangýçta müzik çalýnmasý isteniyorsa
        if (playMusicOnStart && musicSource != null && backgroundMusic != null)
        {
            PlayBackgroundMusic();
        }
    }

    private void SetupAudioSource()
    {
        // Mevcut AudioSource bileþenini kontrol et
        musicSource = GetComponent<AudioSource>();

        // AudioSource yoksa oluþtur
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }

        // AudioSource ayarlarýný yapýlandýr
        musicSource.loop = true; // Sürekli tekrar etsin
        musicSource.playOnAwake = false; // Otomatik baþlamasýn
        musicSource.volume = musicVolume; // Ses seviyesini ayarla
        musicSource.clip = backgroundMusic; // Müziði ayarla
    }

    private void InitializeSliders()
    {
        // Müzik slider'ýný ayarla
        if (musicVolumeSlider != null)
        {
            // Slider'ýn deðerini mevcut ses seviyesine eþitle
            musicVolumeSlider.value = musicSource.volume;

            // Deðer deðiþtiðinde ses seviyesini güncelle
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);

            Debug.Log("Müzik slider'ý baþlatýldý: " + musicVolumeSlider.value);
        }

        // Efekt sesi slider'ýný ayarla (opsiyonel)
        if (sfxVolumeSlider != null && sfxAudioSource != null)
        {
            sfxVolumeSlider.value = sfxAudioSource.volume;
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        }
    }

    // Arkaplan müziðini çal
    public void PlayBackgroundMusic()
    {
        if (musicSource != null && !musicSource.isPlaying)
        {
            musicSource.Play();
            Debug.Log("Arkaplan müziði baþlatýldý");
        }
    }

    // Arkaplan müziðini durdur
    public void StopBackgroundMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
            Debug.Log("Arkaplan müziði durduruldu");
        }
    }

    // Arkaplan müziðini duraklat
    public void PauseBackgroundMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Pause();
            Debug.Log("Arkaplan müziði duraklatýldý");
        }
    }

    public delegate void MusicVolumeChanged(float newVolume);
    public static event MusicVolumeChanged OnMusicVolumeChanged;


    // Arkaplan müziðinin ses seviyesini ayarla (0.0f - 1.0f)
    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
        {
            musicSource.volume = Mathf.Clamp01(volume);

            // Slider deðerini güncelle (eðer bu metod slider dýþýndan çaðrýlýrsa)
            if (musicVolumeSlider != null && Mathf.Abs(musicVolumeSlider.value - volume) > 0.01f)
                musicVolumeSlider.value = volume;

            // Event'i tetikle
            OnMusicVolumeChanged?.Invoke(volume);

            Debug.Log($"Arkaplan müziði ses seviyesi: {musicSource.volume}");

            // PlayerPrefs ile kaydet (isteðe baðlý)
            PlayerPrefs.SetFloat("MusicVolume", volume);
            PlayerPrefs.Save();


        }
    }

    // Ses efektlerinin seviyesini ayarla (opsiyonel)
    public void SetSFXVolume(float volume)
    {
        if (sfxAudioSource != null)
        {
            sfxAudioSource.volume = Mathf.Clamp01(volume);

            // PlayerPrefs ile kaydet (isteðe baðlý)
            PlayerPrefs.SetFloat("SFXVolume", volume);
            PlayerPrefs.Save();
        }
    }

    // Mevcut ses seviyesini al
    public float GetMusicVolume()
    {
        return musicSource != null ? musicSource.volume : 0f;
    }

    // Kayýtlý ses ayarlarýný yükle
    private void LoadAudioSettings()
    {
        if (PlayerPrefs.HasKey("MusicVolume"))
        {
            float savedVolume = PlayerPrefs.GetFloat("MusicVolume");
            SetMusicVolume(savedVolume);
        }

        if (PlayerPrefs.HasKey("SFXVolume") && sfxAudioSource != null)
        {
            float savedSFXVolume = PlayerPrefs.GetFloat("SFXVolume");
            SetSFXVolume(savedSFXVolume);
        }
    }

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
