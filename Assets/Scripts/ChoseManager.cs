using System.Collections;
using UnityEngine;

public class ChoseManager : MonoBehaviour
{
    [SerializeField] private GameObject panel1; // �lk panel
    [SerializeField] private GameObject panel2; // �kinci panel

    [SerializeField] private float fadeDuration = 1f; // Fade i�lemi s�resi (Inspector'dan ayarlanabilir)

    private CanvasGroup panel1CanvasGroup;
    private CanvasGroup panel2CanvasGroup;

    void Start()
    {
        // Panel ba�lang�� durumlar�
        if (panel1 != null)
        {
            panel1CanvasGroup = panel1.GetComponent<CanvasGroup>();
            if (panel1CanvasGroup == null)
                panel1CanvasGroup = panel1.AddComponent<CanvasGroup>();

            panel1CanvasGroup.alpha = 0; // Ba�lang��ta g�r�nmez
            panel1.SetActive(false);
        }

        if (panel2 != null)
        {
            panel2CanvasGroup = panel2.GetComponent<CanvasGroup>();
            if (panel2CanvasGroup == null)
                panel2CanvasGroup = panel2.AddComponent<CanvasGroup>();

            panel2CanvasGroup.alpha = 0; // Ba�lang��ta g�r�nmez
            panel2.SetActive(false);
        }
    }

    // �lk paneli a�an metod
    public void OpenPanel1()
    {
        if (panel1 != null)
        {
            panel1.SetActive(true);
            StartCoroutine(FadeIn(panel1CanvasGroup, fadeDuration)); // S�reyi Inspector'dan al
        }
    }

    // �kinci paneli a�an metod
    public void OpenPanel2()
    {
        if (panel2 != null)
        {
            panel2.SetActive(true);
            StartCoroutine(FadeIn(panel2CanvasGroup, fadeDuration)); // S�reyi Inspector'dan al
        }
    }

    // Fade-in i�lemi (alfa de�erini 0'dan 1'e ��kar�r)
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

        canvasGroup.alpha = 1f; // Tamamen g�r�n�r yap
    }

    // Fade-out i�lemi (alfa de�erini 1'den 0'a d���r�r)
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

        canvasGroup.alpha = 0f; // Tamamen g�r�nmez yap
        canvasGroup.gameObject.SetActive(false); // Paneli devre d��� b�rak
    }
}
