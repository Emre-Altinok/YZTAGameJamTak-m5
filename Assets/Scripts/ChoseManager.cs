using System.Collections;
using UnityEngine;

public class ChoseManager : MonoBehaviour
{
    [SerializeField] private GameObject panel1; // Ýlk panel
    [SerializeField] private GameObject panel2; // Ýkinci panel

    [SerializeField] private float fadeDuration = 1f; // Fade iþlemi süresi (Inspector'dan ayarlanabilir)

    private CanvasGroup panel1CanvasGroup;
    private CanvasGroup panel2CanvasGroup;

    void Start()
    {
        // Panel baþlangýç durumlarý
        if (panel1 != null)
        {
            panel1CanvasGroup = panel1.GetComponent<CanvasGroup>();
            if (panel1CanvasGroup == null)
                panel1CanvasGroup = panel1.AddComponent<CanvasGroup>();

            panel1CanvasGroup.alpha = 0; // Baþlangýçta görünmez
            panel1.SetActive(false);
        }

        if (panel2 != null)
        {
            panel2CanvasGroup = panel2.GetComponent<CanvasGroup>();
            if (panel2CanvasGroup == null)
                panel2CanvasGroup = panel2.AddComponent<CanvasGroup>();

            panel2CanvasGroup.alpha = 0; // Baþlangýçta görünmez
            panel2.SetActive(false);
        }
    }

    // Ýlk paneli açan metod
    public void OpenPanel1()
    {
        if (panel1 != null)
        {
            panel1.SetActive(true);
            StartCoroutine(FadeIn(panel1CanvasGroup, fadeDuration)); // Süreyi Inspector'dan al
        }
    }

    // Ýkinci paneli açan metod
    public void OpenPanel2()
    {
        if (panel2 != null)
        {
            panel2.SetActive(true);
            StartCoroutine(FadeIn(panel2CanvasGroup, fadeDuration)); // Süreyi Inspector'dan al
        }
    }

    // Fade-in iþlemi (alfa deðerini 0'dan 1'e çýkarýr)
    private IEnumerator FadeIn(CanvasGroup canvasGroup, float duration)
    {
        float startAlpha = canvasGroup.alpha;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, elapsedTime / duration);
            yield return null;
        }

        canvasGroup.alpha = 1f; // Tamamen görünür yap
    }

    // Fade-out iþlemi (alfa deðerini 1'den 0'a düþürür)
    public IEnumerator FadeOut(CanvasGroup canvasGroup, float duration)
    {
        float startAlpha = canvasGroup.alpha;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / duration);
            yield return null;
        }

        canvasGroup.alpha = 0f; // Tamamen görünmez yap
        canvasGroup.gameObject.SetActive(false); // Paneli devre dýþý býrak
    }
}
