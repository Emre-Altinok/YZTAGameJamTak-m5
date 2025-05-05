using UnityEngine;

public class Gold : MonoBehaviour
{
    public ScoreManager scoreManager; // ScoreManager referansı
    public float goldValue = 10f;     // Altının verdiği değer
    public AudioClip collectSound;   // Toplama sesi
    private AudioSource audioSource; // Ses kaynağı
    private bool isCollected = false; // Altının toplanıp toplanmadığını kontrol etmek için

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
        // Eğer çarpışan obje "Player" tag'ine sahipse ve daha önce toplanmadıysa
        if (other.CompareTag("Player") && !isCollected)
        {
            Debug.Log("TRIGGERA GİRDİK");
            isCollected = true; // Altın toplandı olarak işaretle

            // Skoru artır
            if (scoreManager != null)
            {
                scoreManager.ScoreAdd(goldValue);
            }

            // Görsel komponenti devre dışı bırak (altını görünmez yap)
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }

            // Tüm Collider'ları devre dışı bırak
            Collider2D[] colliders = GetComponents<Collider2D>();
            foreach (var collider in colliders)
            {
                collider.enabled = false;
            }

            // Ses çal
            if (collectSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(collectSound);

                // Ses bittikten sonra objeyi tamamen yok et
                Destroy(gameObject, collectSound.length);
            }
            else
            {
                // Ses yoksa hemen yok et
                Destroy(gameObject);
            }

            // Debug log mesajı: Altın kayboldu
            Debug.Log("Altın kayboldu ve oyuncuya eklendi!");
        }
    }
}
