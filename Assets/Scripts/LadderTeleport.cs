using UnityEngine;

public class LadderTeleport : MonoBehaviour
{
    [SerializeField] private Transform targetPosition; // Hedef pozisyon

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Çarpýþan nesnenin Player olup olmadýðýný kontrol et
        if (collision.CompareTag("Player"))
        {
            // Oyuncuyu hedef pozisyona ýþýnla
            collision.transform.position = targetPosition.position;
            Debug.Log("Player teleported to: " + targetPosition.position);
        }
    }
}
