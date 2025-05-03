using UnityEngine;

public class Gold : MonoBehaviour
{
    public ScoreManager scoreManager; // ScoreManager referansı
    public float goldValue = 10f;     // Altının verdiği değer
    public GameObject goldPickupEffect; // Altın alındığında oynatılacak görsel efekt

    private void OnTriggerEnter(Collider other)
    {
        // Eğer çarpışan obje "Player" tag'ine sahipse
        if (other.CompareTag("Player"))
        {
            // Skoru artır
            scoreManager.ScoreAdd(goldValue);

            // Altın alındığında bir efekt oluştur
            if (goldPickupEffect != null)
            {
                Instantiate(goldPickupEffect, transform.position, Quaternion.identity); // Efekt başlat
            }

            // Altın objesini yok et
            Destroy(gameObject);

            // Debug log mesajı: Altın kayboldu
            Debug.Log("Altın kayboldu ve oyuncuya eklendi!");
        }
    }
}
