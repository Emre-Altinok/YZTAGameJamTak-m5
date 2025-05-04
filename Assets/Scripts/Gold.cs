using UnityEngine;

public class Gold : MonoBehaviour
{
    public ScoreManager scoreManager; // ScoreManager referansı
    public float goldValue = 10f;     // Altının verdiği değer
    public AudioClip collectSound;   // Toplama sesi
    private AudioSource audioSource; // Ses kaynağı

    private void Start()
    {
        // Eğer ScoreManager referansı atanmadıysa, sahnede arama yap
        if (scoreManager == null)
        {
            scoreManager = FindAnyObjectByType<ScoreManager>();
            if (scoreManager == null)
            {
                Debug.LogError("ScoreManager bulunamadı! Lütfen sahnede bir ScoreManager olduğundan emin olun.");
            }
        }

        // AudioSource bileşenini al veya ekle
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // 2D oyunlar için doğru metod
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("TRIGGERA GİRDİK");
        // Eğer çarpışan obje "Player" tag'ine sahipse
        if (other.CompareTag("Player"))
        {
            // Skoru artır
            if (scoreManager != null)
            {
                scoreManager.ScoreAdd(goldValue);
            }

            // Ses çal
            if (collectSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(collectSound);
            }

            // Altın objesini yok et
            Destroy(gameObject, collectSound != null ? collectSound.length : 0f);

            // Debug log mesajı: Altın kayboldu
            Debug.Log("Altın kayboldu ve oyuncuya eklendi!");
        }
    }
}
