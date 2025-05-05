using UnityEngine;

public class ButtonGenerate : MonoBehaviour
{
    [SerializeField] private GameObject uiObject1; // Ýlk UI GameObject
    [SerializeField] private GameObject uiObject2; // Ýkinci UI GameObject

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Eðer trigger alanýna giren obje "Player" tag'ine sahipse
        if (other.CompareTag("Player"))
        {
            // UI GameObject'leri aktif hale getir
            if (uiObject1 != null)
                uiObject1.SetActive(true);

            if (uiObject2 != null)
                uiObject2.SetActive(true);

            Debug.Log("UI GameObject'ler aktif hale getirildi.");
        }
    }
}
