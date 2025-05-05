using UnityEngine;
using System.Collections;

public class AppearText : MonoBehaviour
{
    [SerializeField] private float delay = 3f; // Bekleme süresi
    [Tooltip("True: Enable, False: Disable")] // Tooltip açýklamasý
    [SerializeField] private bool enableDisable = true; // True: Enables, False: Disable

    private void Start()
    {
        // Belirtilen süre sonra durumu deðiþtir
        StartCoroutine(ChangeStateAfterDelay(enableDisable));
    }

    private IEnumerator ChangeStateAfterDelay(bool choice)
    {
        yield return new WaitForSeconds(delay);

        // Süre sonunda objenin durumunu deðiþtir
        gameObject.SetActive(choice);
    }
}
