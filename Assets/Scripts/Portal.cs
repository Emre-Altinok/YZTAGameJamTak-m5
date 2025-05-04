using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    [Header("Portal Ayarlar�")]
    [Tooltip("Portal etkinle�ti�inde �al�nacak ses")]
    [SerializeField] private AudioClip portalSound;

    [Tooltip("Ses efektinin ses seviyesi")]
    [SerializeField] private float volume = 1f;

    [Tooltip("Hangi tag'e sahip objeler portal ile etkile�ime girebilir")]
    [SerializeField] private string targetTag = "Player";

    [Header("Efekt Ayarlar�")]
    [SerializeField] private GameObject portalEffect; // �ste�e ba�l� portal efekti

    private bool isActivated = false;
    private AudioSource audioSource;

    private void Awake()
    {
        // Ses kayna�� olu�tur veya mevcut olan� al
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && portalSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Portal efektini ba�lang��ta kapal� tut
        if (portalEffect != null)
        {
            portalEffect.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // E�er portal etkinle�memi�se ve do�ru tag'e sahip nesne girerse
        if (!isActivated && other.CompareTag(targetTag))
        {
            ActivatePortal();
        }
    }

    private void ActivatePortal()
    {
        isActivated = true;
        Debug.Log("Portal etkinle�tirildi! Sonraki sahneye ge�i� yap�l�yor...");

        // Portal efektini g�ster
        if (portalEffect != null)
        {
            portalEffect.SetActive(true);
        }

        // Portal sesini �al
        if (portalSound != null && audioSource != null)
        {
            audioSource.clip = portalSound;
            audioSource.volume = volume;
            audioSource.Play();
            Debug.Log("Portal sesi �al�n�yor: " + portalSound.name);
        }

        // Sahne ge�i�ini hemen yap
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        // Mevcut sahne indexini al
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // Toplam sahne say�s�n� al
        int sceneCount = SceneManager.sceneCountInBuildSettings;

        // Sonraki sahneyi hesapla (d�ng�sel ge�i� i�in)
        int nextSceneIndex = (currentSceneIndex + 1) % sceneCount;

        // Sonraki sahneyi y�kle
        SceneManager.LoadScene(nextSceneIndex);
        Debug.Log("Ge�i� yap�l�yor. Sonraki sahne: " + nextSceneIndex);
    }

    // �ste�e ba�l�: Portal� d��ar�dan tetiklemek i�in public metod
    public void TriggerPortal()
    {
        if (!isActivated)
        {
            ActivatePortal();
        }
    }
}
