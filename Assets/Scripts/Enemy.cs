using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int health = 10;
    public Transform player; // Oyuncunun pozisyonu
    public float moveSpeed = 3f; // Canavarýn hareket hýzý
    public float followDistance = 5f; // Canavarýn takip mesafesi
    public float attackDistance = 1.5f; // Canavarýn saldýrý mesafesi
    public int attackDamage = 1; // Canavarýn vereceði hasar
    public float attackCooldown = 1f; // Saldýrý soðuma süresi
    private float lastAttackTime = 0f; // Son saldýrý zamaný

    private bool isFollowing = false; // Takip durumu
    private Animator animator; // Canavar animatörü
    private Rigidbody2D rb; // Canavarýn Rigidbody2D bileþeni
    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindWithTag("Player").transform; // Oyuncuyu bul
        }

        animator = GetComponent<Animator>(); // Animator referansýný al
        rb = GetComponent<Rigidbody2D>(); // Rigidbody2D referansýný al
    }
    void Update()
    {
        // Canavar ile oyuncu arasýndaki mesafeyi hesapla
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Oyuncuya yaklaþma durumu
        if (distanceToPlayer < followDistance)
        {
            isFollowing = true;
        }
        else
        {
            isFollowing = false;
        }

        // Takip etme ve saldýrma iþlemleri
        if (isFollowing)
        {
            FollowPlayer(); // Oyuncuyu takip et
        }

        // Oyuncuya saldýrma
        if (distanceToPlayer <= attackDistance && Time.time > lastAttackTime + attackCooldown)
        {
            AttackPlayer(); // Oyuncuya saldýr
        }
    }
        // Oyuncuyu takip etme fonksiyonu
        void FollowPlayer()
        {
            // Oyuncuya doðru yönel
            Vector2 direction = (player.position - transform.position).normalized;

            // Canavarý oyuncuya doðru hareket ettir
            transform.Translate(direction * moveSpeed * Time.deltaTime);
        }

        // Oyuncuya saldýrma fonksiyonu
        void AttackPlayer()
        {
            lastAttackTime = Time.time; // Son saldýrý zamanýný güncelle

            // Oyuncuya hasar ver
            PlayerHP playerHP = player.GetComponent<PlayerHP>(); // PlayerHP script’ini al
            if (playerHP != null)
            {
                Vector2 knockbackDirection = new Vector2(player.position.x - transform.position.x, 0).normalized;
                playerHP.TakeDamage(attackDamage, knockbackDirection); // Oyuncuya hasar ver ve knockback uygula
            }

            // Saldýrý animasyonunu tetikle
            animator.SetTrigger("Attack");
            Debug.Log("Canavar oyuncuya saldýrdý!");
        }

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
