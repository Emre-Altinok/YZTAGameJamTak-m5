using UnityEngine;

public class DamageArea : MonoBehaviour
{
    public int damageAmount = 1; // Verilecek hasar miktar�
    public float knockbackDirectionX = -1f; // Knockback y�n� (X ekseni)
    public float knockbackDirectionY = 0.5f; // Knockback y�n� (Y ekseni)

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // �arp��an nesnenin Player olup olmad���n� kontrol et
        PlayerHP playerHP = collision.GetComponent<PlayerHP>();
        if (playerHP != null)
        {
            // Knockback y�n�n� belirle
            Vector2 knockbackDirection = new Vector2(knockbackDirectionX, knockbackDirectionY).normalized;

            // Hasar ver ve knockback uygula
            playerHP.TakeDamage(damageAmount, knockbackDirection);
            Debug.Log("Player has taken damage: " + damageAmount); // Hasar al�nd���n� konsola yazd�r
        }
    }
}
