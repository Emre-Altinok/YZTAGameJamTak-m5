using UnityEngine;

public class Button : MonoBehaviour
{
    public Animator buttonAnimator;  // Animator referansı
    private bool basildi = false;   // Buton basıldığını kontrol eder

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!basildi && other.CompareTag("Player"))
        {
            basildi = true;  // Buton basıldığını işaretle
            Debug.Log("Butona basıldı!");

            // Animator varsa "Pressed" trigger'ını çalıştır
            if (buttonAnimator != null)
            {
                buttonAnimator.SetTrigger("Pressed");  // Animasyonu tetikle
                buttonAnimator.SetBool("isPressed", true);  
            }
            else
            {
                Debug.LogError("Button animator referansı eksik!");
            }
        }
    }

    // Animasyon bitene kadar butonu tekrar basılabilir yapmamak için:
    private void ResetButton()
    {
        // Buton animasyonu tamamlandıktan sonra basıldı durumunu sıfırla
        basildi = false;
    }
}
