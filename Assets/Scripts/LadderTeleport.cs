using UnityEngine;

public class LadderTeleport : MonoBehaviour
{
    [SerializeField] private Transform targetPosition; // Hedef pozisyon

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // �arp��an nesnenin Player olup olmad���n� kontrol et
        if (collision.CompareTag("Player"))
        {
            // Oyuncuyu hedef pozisyona ���nla
            collision.transform.position = targetPosition.position;
            Debug.Log("Player teleported to: " + targetPosition.position);
        }
    }
}
