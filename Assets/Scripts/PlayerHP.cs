using UnityEngine;
using UnityEngine.UI;
public class PlayerHP : MonoBehaviour
{
    public int maxHealth = 3; // Maksimum can
    private int currentHealth; // Mevcut can
    public Image healthBarFill; // Can barýnýn doluluk kýsmý
    private Animator animator; // Animator referansý
    private Rigidbody2D rb; // Rigidbody2D referansý

    public float knockbackForce = 5f; // Knockback kuvveti

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

    private void ApplyKnockback(Vector2 direction)
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; // Mevcut hareketi sýfýrla
            rb.AddForce(direction * knockbackForce, ForceMode2D.Impulse); // Knockback kuvveti uygula
        }
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
        }
        // Ölüm sonrasý iþlemler (örneðin, oyunu durdurma) buraya eklenebilir
    }

    public int GetCurrentHealth()
    {
        return currentHealth; // Mevcut caný döndür
    }
}
