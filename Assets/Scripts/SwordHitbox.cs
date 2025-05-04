using UnityEngine;

public class SwordHitbox : MonoBehaviour
{
    public int damage = 1; // Hasar� 1'e d���rd�m
    public float knockbackForce = 3f; // Knockback kuvvetini azaltt�m

    private Collider2D swordCollider;
    private Transform playerTransform;
    private bool hasHitEnemy = false; // Her sald�r�da bir kez hasar vermek i�in

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
        hasHitEnemy = false; // Hitbox etkinle�tirildi�inde flag'i s�f�rla

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

        hasHitEnemy = false; // Hitbox devre d��� b�rak�ld���nda flag'i s�f�rla
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // �arp��ma ger�ekle�ti, debug bilgisi
        Debug.Log("OnTriggerEnter2D called in SwordHitbox with: " + other.name);

        // E�er zaten bu sald�r�da d��mana hasar verdiyse, tekrar hasar verme
        if (hasHitEnemy) return;

        // D��man ile �arp��ma kontrol�
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                // Hasar verme s�n�rlamas�
                hasHitEnemy = true;

                // Knockback y�n�: Oyuncunun bakt��� y�nde, ama sadece X ekseninde
                Vector2 knockbackDirection;

                if (playerTransform != null)
                {
                    // X y�n�n� oyuncunun y�n�ne g�re belirle
                    float playerFacingDirection = playerTransform.localScale.x;

                    // Sadece yatay knockback uygula, dikey kuvvet �ok az olsun
                    knockbackDirection = new Vector2(playerFacingDirection, 0.05f).normalized;
                }
                else
                {
                    // Y�n bilgisi yoksa, d��man�n pozisyonundan uzakla�t�r
                    knockbackDirection = (other.transform.position - transform.position).normalized;
                    knockbackDirection.y = 0.05f; // Y ekseninde �ok az kuvvet
                }

                // D��mana hasar ve knockback uygula
                enemy.TakeDamage(damage, knockbackDirection * knockbackForce);
                Debug.Log($"Enemy hit by sword! Damage: {damage}, Knockback: {knockbackDirection}");
            }
        }
    }
}
