using UnityEngine;

public class SwordHitbox : MonoBehaviour
{
    public int damage = 1; // Hasarý 1'e düþürdüm
    public float knockbackForce = 3f; // Knockback kuvvetini azalttým

    private Collider2D swordCollider;
    private Transform playerTransform;
    private bool hasHitEnemy = false; // Her saldýrýda bir kez hasar vermek için

    private void Awake()
    {
        swordCollider = GetComponent<Collider2D>();
        if (swordCollider == null)
        {
            Debug.LogError("SwordHitbox: Collider2D component not found!");
            return;
        }

        if (!swordCollider.isTrigger)
        {
            Debug.LogWarning("SwordHitbox: Collider2D is not set as Trigger! Setting to Trigger.");
            swordCollider.isTrigger = true;
        }

        swordCollider.enabled = false;
        playerTransform = transform.root;
    }

    public void EnableHitbox()
    {
        hasHitEnemy = false; // Hitbox etkinleþtirildiðinde flag'i sýfýrla

        if (swordCollider != null)
        {
            swordCollider.enabled = true;
            Debug.Log("Sword hitbox enabled");
        }
        else
        {
            Debug.LogError("Cannot enable hitbox: swordCollider is null");
        }
    }

    public void DisableHitbox()
    {
        if (swordCollider != null)
        {
            swordCollider.enabled = false;
            Debug.Log("Sword hitbox disabled");
        }

        hasHitEnemy = false; // Hitbox devre dýþý býrakýldýðýnda flag'i sýfýrla
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Çarpýþma gerçekleþti, debug bilgisi
        Debug.Log("OnTriggerEnter2D called in SwordHitbox with: " + other.name);

        // Eðer zaten bu saldýrýda düþmana hasar verdiyse, tekrar hasar verme
        if (hasHitEnemy) return;

        // Düþman ile çarpýþma kontrolü
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                // Hasar verme sýnýrlamasý
                hasHitEnemy = true;

                // Knockback yönü: Oyuncunun baktýðý yönde, ama sadece X ekseninde
                Vector2 knockbackDirection;

                if (playerTransform != null)
                {
                    // X yönünü oyuncunun yönüne göre belirle
                    float playerFacingDirection = playerTransform.localScale.x;

                    // Sadece yatay knockback uygula, dikey kuvvet çok az olsun
                    knockbackDirection = new Vector2(playerFacingDirection, 0.05f).normalized;
                }
                else
                {
                    // Yön bilgisi yoksa, düþmanýn pozisyonundan uzaklaþtýr
                    knockbackDirection = (other.transform.position - transform.position).normalized;
                    knockbackDirection.y = 0.05f; // Y ekseninde çok az kuvvet
                }

                // Düþmana hasar ve knockback uygula
                enemy.TakeDamage(damage, knockbackDirection * knockbackForce);
                Debug.Log($"Enemy hit by sword! Damage: {damage}, Knockback: {knockbackDirection}");
            }
        }
    }
}
