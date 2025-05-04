using UnityEngine;
using UnityEngine.InputSystem;

public class Teleportert : MonoBehaviour
{
    public Transform targetLocation; // Iþýnlanýlacak hedef konum

    private GameObject playerInTrigger = null; // Trigger içindeki oyuncu

    private void Update()
    {
        // E tuþuna basýlýp oyuncu trigger içindeyse ýþýnla
        if (playerInTrigger != null && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            // Oyuncuyu hedef konuma ýþýnla
            playerInTrigger.transform.position = targetLocation.position;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Trigger entered by: " + other.gameObject.name);
        // Sadece "Player" etiketine sahip objeyi kaydet
        if (other.CompareTag("Player"))
        {
            playerInTrigger = other.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Oyuncu trigger alanýndan çýktýysa referansý temizle
        if (other.CompareTag("Player") && other.gameObject == playerInTrigger)
        {
            playerInTrigger = null;
        }
    }
}
