using System.Collections;
using UnityEngine;
using UnityEngine.UI;
public class PlayerHP : MonoBehaviour
{
    public int maxHealth = 3; // Maksimum can
    private int currentHealth; // Mevcut can
    public Image healthBarFill; // Can barýnýn doluluk kýsmý
    private Animator animator; // Animator referansý
    private Rigidbody2D rb; // Rigidbody2D referansý

    public float knockbackForce = 10f; // Knockback kuvveti

    void Start()
    {
        currentHealth = maxHealth; // Oyuncunun canýný maksimuma ayarla
        animator = GetComponent<Animator>(); // Animator bileþenini al
        rb = GetComponent<Rigidbody2D>(); // Rigidbody2D bileþenini al
    }

    public void TakeDamage(int damage, Vector2 knockbackDirection)
    {
        currentHealth -= damage; // Hasar al
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Caný 0 ile maxHealth arasýnda sýnýrla
        Debug.Log("Current Health: " + currentHealth); // Mevcut caný konsola yazdýr
        UpdateHealthBar(); // Can barýný güncelle

        // Knockback uygula
        ApplyKnockback(knockbackDirection);

        if (animator != null)
        {
            animator.SetTrigger("Hit"); // Hit tetikle
            Debug.Log("Hit tetiklendi");
        }
        if (currentHealth <= 0)
        {
            Die(); // Can sýfýrsa öl
            Debug.Log("Player is dead"); // Ölüm durumunu konsola yazdýr
        }
    }

    // PlayerHP.cs dosyasýnda ApplyKnockback metodunu güncellemelisiniz:
    private void ApplyKnockback(Vector2 direction)
    {
        // Eðer knockback yönü Vector2.zero ise, knockback uygulanmaz
        if (direction == Vector2.zero || rb == null) return;

        rb.linearVelocity = Vector2.zero; // Mevcut hareketi sýfýrla
        rb.AddForce(direction * knockbackForce, ForceMode2D.Impulse); // Knockback kuvveti uygula
    }

    private void UpdateHealthBar()
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = (float)currentHealth / maxHealth; // Doluluk oranýný hesapla
            Debug.Log("Health bar updated: " + healthBarFill.fillAmount); // Can barý güncellendiðini konsola yazdýr
        }
    }

    private void Die()
    {
        if (animator != null)
        {
            animator.SetTrigger("isDead"); // isDead tetikle
            StartCoroutine(WaitForDeathAnimation()); // Ölüm animasyonunu bekle
        }
        GameManager.Instance.ShowGameOverUI();
        // Input actions'ý devre dýþý býrak
        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.DisableInputActions();
        }
        // GameManager üzerinden GameOver UI'yi aktif et


        Debug.Log("Player is dead");
    }

    private IEnumerator WaitForDeathAnimation()
    {
        // Ölüm animasyonunun tamamlanmasýný bekle
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        while (!stateInfo.IsName("Death") || stateInfo.normalizedTime < 1.0f)
        {
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }

        // Animatörü devre dýþý býrak
        animator.enabled = false;
    }

    public int GetCurrentHealth()
    {
        return currentHealth; // Mevcut caný döndür
    }
}
