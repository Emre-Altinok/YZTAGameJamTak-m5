using UnityEngine;
using System.Collections;

public class TextUp : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 20f; // Yukarý hareket hýzý (piksel/saniye)
    [SerializeField] private float moveDuration = 30f; // Hareket süresi (saniye)
    private bool isMoving = true; // Hareket durumu

    void Start()
    {
        // Hareket süresinden sonra hareketi durdurmak için coroutine baþlat
        StartCoroutine(StopMovementAfterDuration());
    }

    void Update()
    {
        // Eðer hareket aktifse yukarý doðru kay
        if (isMoving)
        {
            // Her karede yukarý doðru hareket et
            transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);
        }
    }

    private IEnumerator StopMovementAfterDuration()
    {
        // Belirtilen süre kadar bekle
        yield return new WaitForSeconds(moveDuration);

        // Süre dolunca hareketi durdur
        isMoving = false;
    }
}
