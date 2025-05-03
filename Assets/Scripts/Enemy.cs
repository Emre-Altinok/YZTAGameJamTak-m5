using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int health = 2;
    public Transform player; // Oyuncunun pozisyonu
    public float moveSpeed = 3f; // Canavar�n hareket h�z�
    public float followDistance = 5f; // Canavar�n takip mesafesi
    public float attackDistance = 1.5f; // Canavar�n sald�r� mesafesi
    public float retreatDistance = 0.8f; // Oyuncuya �ok yakla��ld���nda geri �ekilme mesafesi
    public int attackDamage = 1; // Canavar�n verece�i hasar
    public float attackCooldown = 1f; // Sald�r� so�uma s�resi
    private float lastAttackTime = 0f; // Son sald�r� zaman�

    private bool isFollowing = false; // Takip durumu
    private Animator animator; // Canavar animat�r�
    private Rigidbody2D rb; // Canavar�n Rigidbody2D bile�eni
    private Collider2D[] colliders; // T�m �arp��ma bile�enleri
    private SpriteRenderer spriteRenderer; // SpriteRenderer referans�

    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindWithTag("Player").transform; // Oyuncuyu bul
        }

        animator = GetComponent<Animator>(); // Animator referans�n� al
        rb = GetComponent<Rigidbody2D>(); // Rigidbody2D referans�n� al
        colliders = GetComponents<Collider2D>(); // T�m Collider2D bile�enlerini al
        spriteRenderer = GetComponent<SpriteRenderer>(); // SpriteRenderer referans�n� al
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

        // Animasyon i�in h�z parametresini g�ncelle
        animator.SetFloat("Speed", isFollowing ? moveSpeed : 0f);
    }

    void FollowPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;

        // Sprite'� hareket y�n�ne g�re �evir
        if (direction.x > 0)
        {
            spriteRenderer.flipX = false; // Sa� tarafa bak
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

        // Sprite'� hareket y�n�ne g�re �evir
        if (direction.x > 0)
        {
            spriteRenderer.flipX = false; // Sa� tarafa bak
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

        // Sald�r� animasyonunu tetikle
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

        // �l�m animasyonunu tetikle
        animator.SetBool("isDead", true);

        // T�m animat�r ve �arp��ma bile�enlerini devre d��� b�rak
        animator.enabled = false;
        rb.simulated = false; // Rigidbody2D'yi devre d��� b�rak
        foreach (var collider in colliders)
        {
            collider.enabled = false; // T�m Collider2D bile�enlerini devre d��� b�rak
        }

        // GameObject'i yok etmeden �nce bir s�re bekle (iste�e ba�l�)
        Destroy(gameObject, 2f); // 2 saniye sonra yok et
    }
}
