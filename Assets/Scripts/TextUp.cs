using UnityEngine;
using System.Collections;

public class TextUp : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 20f; // Yukar� hareket h�z� (piksel/saniye)
    [SerializeField] private float moveDuration = 30f; // Hareket s�resi (saniye)
    private bool isMoving = true; // Hareket durumu

    void Start()
    {
        // Hareket s�resinden sonra hareketi durdurmak i�in coroutine ba�lat
        StartCoroutine(StopMovementAfterDuration());
    }

    void Update()
    {
        // E�er hareket aktifse yukar� do�ru kay
        if (isMoving)
        {
            // Her karede yukar� do�ru hareket et
            transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);
        }
    }

    private IEnumerator StopMovementAfterDuration()
    {
        // Belirtilen s�re kadar bekle
        yield return new WaitForSeconds(moveDuration);

        // S�re dolunca hareketi durdur
        isMoving = false;
    }
}
