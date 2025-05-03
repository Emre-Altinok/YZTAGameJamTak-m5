using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int health = 2;
    public Transform player; // Oyuncunun pozisyonu
    public float moveSpeed = 3f; // Canavarýn hareket hýzý
    public float followDistance = 5f; // Canavarýn takip mesafesi
    public float attackDistance = 1.5f; // Canavarýn saldýrý mesafesi
    public float retreatDistance = 0.8f; // Oyuncuya çok yaklaþýldýðýnda geri çekilme mesafesi
    public int attackDamage = 1; // Canavarýn vereceði hasar
    public float attackCooldown = 1f; // Saldýrý soðuma süresi
    private float lastAttackTime = 0f; // Son saldýrý zamaný

    private bool isFollowing = false; // Takip durumu
    private Animator animator; // Canavar animatörü
    private Rigidbody2D rb; // Canavarýn Rigidbody2D bileþeni
    private Collider2D[] colliders; // Tüm çarpýþma bileþenleri
    private SpriteRenderer spriteRenderer; // SpriteRenderer referansý

    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindWithTag("Player").transform; // Oyuncuyu bul
        }

        animator = GetComponent<Animator>(); // Animator referansýný al
        rb = GetComponent<Rigidbody2D>(); // Rigidbody2D referansýný al
        colliders = GetComponents<Collider2D>(); // Tüm Collider2D bileþenlerini al
        spriteRenderer = GetComponent<SpriteRenderer>(); // SpriteRenderer referansýný al
    }

    void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < followDistance && distanceToPlayer > attackDistance)
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
        else if (distanceToPlayer <= retreatDistance)
        {
            RetreatFromPlayer();
        }

        if (distanceToPlayer <= attackDistance && Time.time > lastAttackTime + attackCooldown)
        {
            AttackPlayer();
        }

        // Animasyon için hýz parametresini güncelle
        animator.SetFloat("Speed", isFollowing ? moveSpeed : 0f);
    }

    void FollowPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;

        // Sprite'ý hareket yönüne göre çevir
        if (direction.x > 0)
        {
            spriteRenderer.flipX = false; // Sað tarafa bak
        }
        else if (direction.x < 0)
        {
            spriteRenderer.flipX = true; // Sol tarafa bak
        }

        transform.Translate(direction * moveSpeed * Time.deltaTime);
        Debug.Log("Enemy is following the player.");
    }

    void RetreatFromPlayer()
    {
        Vector2 direction = (transform.position - player.position).normalized;

        // Sprite'ý hareket yönüne göre çevir
        if (direction.x > 0)
        {
            spriteRenderer.flipX = false; // Sað tarafa bak
        }
        else if (direction.x < 0)
        {
            spriteRenderer.flipX = true; // Sol tarafa bak
        }

        transform.Translate(direction * moveSpeed * Time.deltaTime);
        Debug.Log("Enemy is retreating from the player.");
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

        // Saldýrý animasyonunu tetikle
        animator.SetTrigger("Attack");
    }

    public void TakeDamage(int damage, Vector2 knockbackDirection)
    {
        health -= damage;
        Debug.Log("Enemy took " + damage + " damage! Remaining health: " + health);

        // Knockback uygula
        rb.AddForce(knockbackDirection * 5f, ForceMode2D.Impulse);
        Debug.Log("Enemy knocked back!");

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Enemy is dead!");

        // Ölüm animasyonunu tetikle
        animator.SetBool("isDead", true);

        // Tüm animatör ve çarpýþma bileþenlerini devre dýþý býrak
        animator.enabled = false;
        rb.simulated = false; // Rigidbody2D'yi devre dýþý býrak
        foreach (var collider in colliders)
        {
            collider.enabled = false; // Tüm Collider2D bileþenlerini devre dýþý býrak
        }

        // GameObject'i yok etmeden önce bir süre bekle (isteðe baðlý)
        Destroy(gameObject, 2f); // 2 saniye sonra yok et
    }
}
