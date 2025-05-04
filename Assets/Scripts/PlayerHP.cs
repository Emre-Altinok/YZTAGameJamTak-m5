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

    public float knockbackForce = 10f; // Knockback kuvveti

    void Start()
    {
        currentHealth = maxHealth; // Oyuncunun can�n� maksimuma ayarla
        animator = GetComponent<Animator>(); // Animator bile�enini al
        rb = GetComponent<Rigidbody2D>(); // Rigidbody2D bile�enini al
    }

    public void TakeDamage(int damage, Vector2 knockbackDirection)
    {
        currentHealth -= damage; // Hasar al
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Can� 0 ile maxHealth aras�nda s�n�rla
        Debug.Log("Current Health: " + currentHealth); // Mevcut can� konsola yazd�r
        UpdateHealthBar(); // Can bar�n� g�ncelle

        // Knockback uygula
        ApplyKnockback(knockbackDirection);

        if (animator != null)
        {
            animator.SetTrigger("Hit"); // Hit tetikle
            Debug.Log("Hit tetiklendi");
        }
        if (currentHealth <= 0)
        {
            Die(); // Can s�f�rsa �l
            Debug.Log("Player is dead"); // �l�m durumunu konsola yazd�r
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

    private void Die()
    {
        if (animator != null)
        {
            animator.SetTrigger("isDead"); // isDead tetikle
            StartCoroutine(WaitForDeathAnimation()); // �l�m animasyonunu bekle
        }
        GameManager.Instance.ShowGameOverUI();
        // Input actions'� devre d��� b�rak
        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.DisableInputActions();
        }
        // GameManager �zerinden GameOver UI'yi aktif et


        Debug.Log("Player is dead");
    }

    private IEnumerator WaitForDeathAnimation()
    {
        // �l�m animasyonunun tamamlanmas�n� bekle
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        while (!stateInfo.IsName("Death") || stateInfo.normalizedTime < 1.0f)
        {
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }

        // Animat�r� devre d��� b�rak
        animator.enabled = false;
    }

    public int GetCurrentHealth()
    {
        return currentHealth; // Mevcut can� d�nd�r
    }
}
