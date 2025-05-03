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
        // Canavar ile oyuncu aras�ndaki mesafeyi hesapla
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Oyuncuya yakla�ma durumu
        if (distanceToPlayer < followDistance)
        {
            isFollowing = true;
        }
        else
        {
            isFollowing = false;
        }

        // Takip etme ve sald�rma i�lemleri
        if (isFollowing)
        {
            FollowPlayer(); // Oyuncuyu takip et
        }

        // Oyuncuya sald�rma
        if (distanceToPlayer <= attackDistance && Time.time > lastAttackTime + attackCooldown)
        {
            AttackPlayer(); // Oyuncuya sald�r
        }
    }
        // Oyuncuyu takip etme fonksiyonu
        void FollowPlayer()
        {
            // Oyuncuya do�ru y�nel
            Vector2 direction = (player.position - transform.position).normalized;

            // Canavar� oyuncuya do�ru hareket ettir
            transform.Translate(direction * moveSpeed * Time.deltaTime);
        }

        // Oyuncuya sald�rma fonksiyonu
        void AttackPlayer()
        {
            lastAttackTime = Time.time; // Son sald�r� zaman�n� g�ncelle

            // Oyuncuya hasar ver
            PlayerHP playerHP = player.GetComponent<PlayerHP>(); // PlayerHP script�ini al
            if (playerHP != null)
            {
                Vector2 knockbackDirection = new Vector2(player.position.x - transform.position.x, 0).normalized;
                playerHP.TakeDamage(attackDamage, knockbackDirection); // Oyuncuya hasar ver ve knockback uygula
            }

            // Sald�r� animasyonunu tetikle
            animator.SetTrigger("Attack");
            Debug.Log("Canavar oyuncuya sald�rd�!");
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
        // �l�m animasyonu veya yok etme
        Destroy(gameObject);
    }
}
