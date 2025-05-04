using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject settingsPanel;

    [Header("UI Controls")]
    [SerializeField] private Slider musicVolumeSlider; // M�zik ses seviyesi slider'�
    [SerializeField] private Slider sfxVolumeSlider; // Opsiyonel: Efekt ses seviyesi slider'�

    [Header("Audio Settings")]
    [SerializeField] private AudioClip backgroundMusic; // Arkaplan m�zi�i
    [SerializeField] private float musicVolume = 0.5f; // Varsay�lan ses d�zeyi (0.0f - 1.0f)
    [SerializeField] private bool playMusicOnStart = true; // Ba�lang��ta m�zik �als�n m�
    [SerializeField] private bool dontDestroyOnLoad = false; // Sahneler aras� ge�i�lerde korunsun mu

    // Opsiyonel: Ses efektleri i�in ayr� AudioSource
    [SerializeField] private AudioSource sfxAudioSource;

    private AudioSource musicSource; // M�zik ses kayna��
    private static UIManager instance; // Singleton i�in

    // Singleton instance'�na public eri�im
    public static UIManager Instance => instance;

    private void Awake()
    {
        // Singleton pattern - tek bir UIManager olmas�n� sa�lar
        if (instance == null)
        {
            instance = this;

            // Sahneler aras� ge�i�lerde korunsun mu ayar�
            if (dontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Ba�ka bir UIManager varsa, bunu yok et
            Destroy(gameObject);
            return;
        }

        // M�zik kayna��n� ayarla
        SetupAudioSource();
    }

    private void Start()
    {
        // Slider'lar� ba�lat
        InitializeSliders();

        // E�er ba�lang��ta m�zik �al�nmas� isteniyorsa
        if (playMusicOnStart && musicSource != null && backgroundMusic != null)
        {
            PlayBackgroundMusic();
        }
    }

    private void SetupAudioSource()
    {
        // Mevcut AudioSource bile�enini kontrol et
        musicSource = GetComponent<AudioSource>();

        // AudioSource yoksa olu�tur
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }

        // AudioSource ayarlar�n� yap�land�r
        musicSource.loop = true; // S�rekli tekrar etsin
        musicSource.playOnAwake = false; // Otomatik ba�lamas�n
        musicSource.volume = musicVolume; // Ses seviyesini ayarla
        musicSource.clip = backgroundMusic; // M�zi�i ayarla
    }

    private void InitializeSliders()
    {
        // M�zik slider'�n� ayarla
        if (musicVolumeSlider != null)
        {
            // Slider'�n de�erini mevcut ses seviyesine e�itle
            musicVolumeSlider.value = musicSource.volume;

            // De�er de�i�ti�inde ses seviyesini g�ncelle
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);

            Debug.Log("M�zik slider'� ba�lat�ld�: " + musicVolumeSlider.value);
        }

        // Efekt sesi slider'�n� ayarla (opsiyonel)
        if (sfxVolumeSlider != null && sfxAudioSource != null)
        {
            sfxVolumeSlider.value = sfxAudioSource.volume;
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        }
    }

    // Arkaplan m�zi�ini �al
    public void PlayBackgroundMusic()
    {
        if (musicSource != null && !musicSource.isPlaying)
        {
            musicSource.Play();
            Debug.Log("Arkaplan m�zi�i ba�lat�ld�");
        }
    }

    // Arkaplan m�zi�ini durdur
    public void StopBackgroundMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
            Debug.Log("Arkaplan m�zi�i durduruldu");
        }
    }

    // Arkaplan m�zi�ini duraklat
    public void PauseBackgroundMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Pause();
            Debug.Log("Arkaplan m�zi�i duraklat�ld�");
        }
    }

    public delegate void MusicVolumeChanged(float newVolume);
    public static event MusicVolumeChanged OnMusicVolumeChanged;


    // Arkaplan m�zi�inin ses seviyesini ayarla (0.0f - 1.0f)
    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
        {
            musicSource.volume = Mathf.Clamp01(volume);

            // Slider de�erini g�ncelle (e�er bu metod slider d���ndan �a�r�l�rsa)
            if (musicVolumeSlider != null && Mathf.Abs(musicVolumeSlider.value - volume) > 0.01f)
                musicVolumeSlider.value = volume;

            // Event'i tetikle
            OnMusicVolumeChanged?.Invoke(volume);

            Debug.Log($"Arkaplan m�zi�i ses seviyesi: {musicSource.volume}");

            // PlayerPrefs ile kaydet (iste�e ba�l�)
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

            // PlayerPrefs ile kaydet (iste�e ba�l�)
            PlayerPrefs.SetFloat("SFXVolume", volume);
            PlayerPrefs.Save();
        }
    }

    // Mevcut ses seviyesini al
    public float GetMusicVolume()
    {
        return musicSource != null ? musicSource.volume : 0f;
    }

    // Kay�tl� ses ayarlar�n� y�kle
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
