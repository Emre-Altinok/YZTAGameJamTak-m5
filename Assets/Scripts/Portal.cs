using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    [Header("Portal Ayarlarý")]
    [Tooltip("Portal etkinleþtiðinde çalýnacak ses")]
    [SerializeField] private AudioClip portalSound;

    [Tooltip("Ses efektinin ses seviyesi")]
    [SerializeField] private float volume = 1f;

    [Tooltip("Hangi tag'e sahip objeler portal ile etkileþime girebilir")]
    [SerializeField] private string targetTag = "Player";

    [Header("Efekt Ayarlarý")]
    [SerializeField] private GameObject portalEffect; // Ýsteðe baðlý portal efekti

    private bool isActivated = false;
    private AudioSource audioSource;

    private void Awake()
    {
        // Ses kaynaðý oluþtur veya mevcut olaný al
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && portalSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Portal efektini baþlangýçta kapalý tut
        if (portalEffect != null)
        {
            portalEffect.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Eðer portal etkinleþmemiþse ve doðru tag'e sahip nesne girerse
        if (!isActivated && other.CompareTag(targetTag))
        {
            ActivatePortal();
        }
    }

    private void ActivatePortal()
    {
        isActivated = true;
        Debug.Log("Portal etkinleþtirildi! Sonraki sahneye geçiþ yapýlýyor...");

        // Portal efektini göster
        if (portalEffect != null)
        {
            portalEffect.SetActive(true);
        }

        // Portal sesini çal
        if (portalSound != null && audioSource != null)
        {
            audioSource.clip = portalSound;
            audioSource.volume = volume;
            audioSource.Play();
            Debug.Log("Portal sesi çalýnýyor: " + portalSound.name);
        }

        // Sahne geçiþini hemen yap
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        // Mevcut sahne indexini al
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // Toplam sahne sayýsýný al
        int sceneCount = SceneManager.sceneCountInBuildSettings;

        // Sonraki sahneyi hesapla (döngüsel geçiþ için)
        int nextSceneIndex = (currentSceneIndex + 1) % sceneCount;

        // Sonraki sahneyi yükle
        SceneManager.LoadScene(nextSceneIndex);
        Debug.Log("Geçiþ yapýlýyor. Sonraki sahne: " + nextSceneIndex);
    }

    // Ýsteðe baðlý: Portalý dýþarýdan tetiklemek için public metod
    public void TriggerPortal()
    {
        if (!isActivated)
        {
            ActivatePortal();
        }
    }
}
