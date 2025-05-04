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
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        Debug.Log($"PlayerHP: Baþlatýldý - currentHealth: {currentHealth}/{maxHealth}");

        // PlayerController bileþenini kontrol et
        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("PlayerHP: Ayný GameObject'te PlayerController bulunamadý!");
        }
        else
        {
            Debug.Log("PlayerHP: PlayerController baþarýyla bulundu");
        }

        // HealthBar kontrolü
        if (healthBarFill == null)
        {
            Debug.LogError("PlayerHP: healthBarFill atanmamýþ! Can barý görüntülenemeyecek.");
        }
    }

    // PlayerHP.cs içindeki TakeDamage metodunda:
    public void TakeDamage(int damage, Vector2 knockbackDirection)
    {
        Debug.Log($"PlayerHP: TakeDamage çaðrýldý - damage: {damage}, knockbackDir: {knockbackDirection}");

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        Debug.Log("Current Health: " + currentHealth);
        Debug.Log($"PlayerHP: Hasar alýndý. Yeni saðlýk: {currentHealth}/{maxHealth}");


        UpdateHealthBar();

        // Knockback uygula
        ApplyKnockback(knockbackDirection);

        // PlayerController'a hasar alma bilgisini ilet
        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            Debug.Log("PlayerHP: PlayerController.TriggerHit çaðrýlýyor");

            playerController.TriggerHit(); // Hasar alma animasyonu ve sesini tetikle
        }
        else if (animator != null)
        {
            Debug.LogError("PlayerHP: PlayerController bulunamadý!");

            animator.SetTrigger("Hit"); // Eski yöntem
        }

        if (currentHealth <= 0)
        {
            Debug.Log("PlayerHP: Saðlýk sýfýrlandý, ölüm fonksiyonu çaðrýlýyor");

            Die();
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

    // PlayerHP.cs içindeki Die metodunda:
    private void Die()
    {
        // PlayerController'a ölüm bilgisini ilet
        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.SetDeathState(); // Ölüm animasyonu, sesi ve diðer iþlemler
        }
        else if (animator != null)
        {
            animator.SetTrigger("Death"); // Eski yöntem - artýk trigger kullanýyoruz
        }

        // Input actions'ý devre dýþý býrak
        if (playerController != null)
        {
            playerController.DisableInputActions();
        }

        GameManager.Instance.ShowGameOverUI();
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
