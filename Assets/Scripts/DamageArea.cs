using UnityEngine;

public class DamageArea : MonoBehaviour
{
    public int damageAmount = 1; // Verilecek hasar miktarý
    public float knockbackDirectionX = -1f; // Knockback yönü (X ekseni)
    public float knockbackDirectionY = 0.5f; // Knockback yönü (Y ekseni)

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Çarpýþan nesnenin Player olup olmadýðýný kontrol et
        PlayerHP playerHP = collision.GetComponent<PlayerHP>();
        if (playerHP != null)
        {
            // Knockback yönünü belirle
            Vector2 knockbackDirection = new Vector2(knockbackDirectionX, knockbackDirectionY).normalized;

            // Hasar ver ve knockback uygula
            playerHP.TakeDamage(damageAmount, knockbackDirection);
            Debug.Log("Player has taken damage: " + damageAmount); // Hasar alýndýðýný konsola yazdýr
        }
    }
}
