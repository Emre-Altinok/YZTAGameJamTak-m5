using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int health = 10;
    public Transform player; // Oyuncunun pozisyonu
    public float moveSpeed = 3f; // Canavar�n hareket h�z�
    public float followDistance = 5f; // Canavar�n takip mesafesi
    public float attackDistance = 1.5f; // Canavar�n sald�r� mesafesi
    public int attackDamage = 1; // Canavar�n verece�i hasar
    public float attackCooldown = 1f; // Sald�r� so�uma s�resi
    private float lastAttackTime = 0f; // Son sald�r� zaman�

    private bool isFollowing = false; // Takip durumu
    private Animator animator; // Canavar animat�r�
    private Rigidbody2D rb; // Canavar�n Rigidbody2D bile�eni

    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindWithTag("Player").transform; // Oyuncuyu bul
        }

        animator = GetComponent<Animator>(); // Animator referans�n� al
        rb = GetComponent<Rigidbody2D>(); // Rigidbody2D referans�n� al
    }

    void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < followDistance)
        {
            isFollowing = true;
        }
        else
        {
            isFollowing = false;
        }

        if (isFollowing)
        {
            FollowPlayer();
        }

        if (distanceToPlayer <= attackDistance && Time.time > lastAttackTime + attackCooldown)
        {
            AttackPlayer();
        }
    }

    void FollowPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        transform.Translate(direction * moveSpeed * Time.deltaTime);
    }

    void AttackPlayer()
    {
        lastAttackTime = Time.time;

        PlayerHP playerHP = player.GetComponent<PlayerHP>();
        if (playerHP != null)
        {
            Vector2 knockbackDirection = new Vector2(player.position.x - transform.position.x, 0).normalized;
            playerHP.TakeDamage(attackDamage, knockbackDirection);
            Debug.Log("Enemy attacked the player! Damage: " + attackDamage);
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log("Enemy took " + damage + " damage! Remaining health: " + health);

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Enemy is dead!");
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHP playerHP = collision.gameObject.GetComponent<PlayerHP>();
            if (playerHP != null)
            {
                Vector2 knockbackDirection = new Vector2(collision.transform.position.x - transform.position.x, 0).normalized;
                playerHP.TakeDamage(attackDamage, knockbackDirection);
                Debug.Log("Enemy collided with the player! Damage: " + attackDamage);
            }
        }
    }
}
