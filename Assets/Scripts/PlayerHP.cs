using System.Collections;
using UnityEngine;
using UnityEngine.UI;
public class PlayerHP : MonoBehaviour
{
    public int maxHealth = 3; // Maksimum can
    private int currentHealth; // Mevcut can
    public Image healthBarFill; // Can bar�n�n doluluk k�sm�
    private Animator animator; // Animator referans�
    private Rigidbody2D rb; // Rigidbody2D referans�

    public float knockbackForce = 2f; // Knockback kuvveti

    // KnightController referans�
    private KnightController knightController;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // KnightController referans�n� al
        knightController = GetComponent<KnightController>();

        Debug.Log($"PlayerHP: Ba�lat�ld� - currentHealth: {currentHealth}/{maxHealth}");

        // PlayerController bile�enini kontrol et
        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController == null && knightController == null)
        {
            Debug.LogError("PlayerHP: Ayn� GameObject'te Controller bulunamad�!");
        }
        else
        {
            Debug.Log("PlayerHP: Controller ba�ar�yla bulundu");
        }

        // HealthBar kontrol�
        if (healthBarFill == null)
        {
            Debug.LogError("PlayerHP: healthBarFill atanmam��! Can bar� g�r�nt�lenemeyecek.");
        }
    }

    // PlayerHP.cs i�indeki TakeDamage metodunda:
    public void TakeDamage(int damage, Vector2 knockbackDirection)
    {
        Debug.Log($"PlayerHP: TakeDamage �a�r�ld� - damage: {damage}, knockbackDir: {knockbackDirection}");

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        Debug.Log("Current Health: " + currentHealth);
        Debug.Log($"PlayerHP: Hasar al�nd�. Yeni sa�l�k: {currentHealth}/{maxHealth}");

        UpdateHealthBar();

        // Knockback uygula
        ApplyKnockback(knockbackDirection);

        // Controller tipine g�re hasar alma bilgisini ilet
        if (knightController != null)
        {
            // KnightController i�in hasar bildirimi
            knightController.TakeDamage(damage, knockbackDirection);
        }
        else
        {
            // Eski PlayerController i�in hasar bildirimi
            PlayerController playerController = GetComponent<PlayerController>();
            if (playerController != null)
            {
                Debug.Log("PlayerHP: PlayerController.TriggerHit �a�r�l�yor");
                playerController.TriggerHit(); // Hasar alma animasyonu ve sesini tetikle
            }
            else if (animator != null)
            {
                Debug.LogError("PlayerHP: Controller bulunamad�!");
                animator.SetTrigger("Hit"); // Eski y�ntem
            }
        }

        if (currentHealth <= 0)
        {
            Debug.Log("PlayerHP: Sa�l�k s�f�rland�, �l�m fonksiyonu �a�r�l�yor");
            Die();
        }
    }

    // PlayerHP.cs dosyas�nda ApplyKnockback metodunu g�ncellemelisiniz:
    private void ApplyKnockback(Vector2 direction)
    {
        // E�er knockback y�n� Vector2.zero ise, knockback uygulanmaz
        if (direction == Vector2.zero || rb == null) return;

        rb.linearVelocity = Vector2.zero; // Mevcut hareketi s�f�rla
        rb.AddForce(direction * knockbackForce, ForceMode2D.Impulse); // Knockback kuvveti uygula
    }

    private void UpdateHealthBar()
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = (float)currentHealth / maxHealth; // Doluluk oran�n� hesapla
            Debug.Log("Health bar updated: " + healthBarFill.fillAmount); // Can bar� g�ncellendi�ini konsola yazd�r
        }
    }

    // PlayerHP.cs i�indeki Die metodunda:
    private void Die()
    {
        // Controller tipine g�re �l�m bildirimi
        if (knightController != null)
        {
            // KnightController i�in �l�m bildirimi
            knightController.SetDeathState();
        }
        else
        {
            // Eski PlayerController i�in �l�m bildirimi
            PlayerController playerController = GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.SetDeathState(); // �l�m animasyonu, sesi ve di�er i�lemler
            }
            else if (animator != null)
            {
                // Do�rudan Animator'e eri�im (Knight i�in IsDead bool parametresi kullan)
                if (animator.HasParameter("IsDead"))
                {
                    animator.SetBool("IsDead", true);
                }
                else if (animator.HasParameter("Death"))
                {
                    animator.SetTrigger("Death");
                }
            }

            // Input actions'� devre d��� b�rak
            if (playerController != null)
            {
                playerController.DisableInputActions();
            }
        }

        // GameOver UI g�ster
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ShowGameOverUI();
        }

        Debug.Log("Player is dead");
    }

    private IEnumerator WaitForDeathAnimation()
    {
        // �l�m animasyonunun tamamlanmas�n� bekle
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float timeWaited = 0f;
        float maxWaitTime = 3f; // Maksimum bekleme s�resi

        while ((!stateInfo.IsName("Death") || stateInfo.normalizedTime < 1.0f) && timeWaited < maxWaitTime)
        {
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            timeWaited += Time.deltaTime;
            yield return null;
        }

        // Animat�r� devre d��� b�rak
        animator.enabled = false;
    }

    public int GetCurrentHealth()
    {
        return currentHealth; // Mevcut can� d�nd�r
    }

    // Can de�erini test ama�l� ayarlamak i�in
    public void SetHealth(int health)
    {
        currentHealth = Mathf.Clamp(health, 0, maxHealth);
        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }
}

// Add this extension method to check if an Animator has a specific parameter.  
public static class AnimatorExtensions
{
    public static bool HasParameter(this Animator animator, string paramName)
    {
        foreach (var param in animator.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }
}
