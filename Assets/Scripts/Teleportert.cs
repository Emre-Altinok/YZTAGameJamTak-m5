using UnityEngine;
using UnityEngine.InputSystem;

public class Teleportert : MonoBehaviour
{
    public Transform targetLocation; // I��nlan�lacak hedef konum

    private GameObject playerInTrigger = null; // Trigger i�indeki oyuncu

    private void Update()
    {
        // E tu�una bas�l�p oyuncu trigger i�indeyse ���nla
        if (playerInTrigger != null && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            // Oyuncuyu hedef konuma ���nla
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
        // Oyuncu trigger alan�ndan ��kt�ysa referans� temizle
        if (other.CompareTag("Player") && other.gameObject == playerInTrigger)
        {
            playerInTrigger = null;
        }
    }
}
