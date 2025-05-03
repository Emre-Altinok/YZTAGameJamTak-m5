using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int health = 50;

    public void TakeDamage(int damage)
    {
        // Handle taking damage here
        Debug.Log("Enemy took " + damage + " damage!");
        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // Ölüm animasyonu veya yok etme
        Destroy(gameObject);
    }
}
