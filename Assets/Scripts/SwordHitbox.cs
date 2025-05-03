using UnityEngine;

public class SwordHitbox : MonoBehaviour
{
    public int damage = 10;
    private Collider2D swordCollider;

    private void Awake()
    {
        swordCollider = GetComponent<Collider2D>();
        if (swordCollider == null)
        {
            Debug.LogError("SwordHitbox: Collider2D component not found!");
        }
        swordCollider.enabled = false; // Baþlangýçta kapalý
    }

    public void EnableHitbox()
    {
        if (swordCollider != null)
        {
            swordCollider.enabled = true;
            Debug.Log("Sword hitbox enabled");
        }
    }

    public void DisableHitbox()
    {
        if (swordCollider != null)
        {
            swordCollider.enabled = false;
            Debug.Log("Sword hitbox disabled");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("OnTriggerEnter2D called in SwordHitbox.");
        Debug.Log("Collided with: " + other.name);

        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Collided with an Enemy.");
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                Vector2 knockbackDirection = (other.transform.position - transform.position).normalized;
                enemy.TakeDamage(damage, knockbackDirection);
                Debug.Log("Enemy hit by sword! Damage: " + damage);
            }
            else
            {
                Debug.LogError("Enemy component not found on collided object.");
            }
        }
    }
}
