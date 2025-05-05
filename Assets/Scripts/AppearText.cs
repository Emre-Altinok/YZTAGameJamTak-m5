using UnityEngine;
using System.Collections;

public class AppearText : MonoBehaviour
{
    [SerializeField] private float delay = 3f; // Bekleme s�resi
    [Tooltip("True: Enable, False: Disable")] // Tooltip a��klamas�
    [SerializeField] private bool enableDisable = true; // True: Enables, False: Disable

    private void Start()
    {
        // Belirtilen s�re sonra durumu de�i�tir
        StartCoroutine(ChangeStateAfterDelay(enableDisable));
    }

    private IEnumerator ChangeStateAfterDelay(bool choice)
    {
        yield return new WaitForSeconds(delay);

        // S�re sonunda objenin durumunu de�i�tir
        gameObject.SetActive(choice);
    }
}
