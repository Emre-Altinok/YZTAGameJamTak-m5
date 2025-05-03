using UnityEngine;

public class Canavar : MonoBehaviour
{
    public Transform player; // Oyuncunun pozisyonu
    public float moveSpeed = 3f; // Canavarın hareket hızı
    public float followDistance = 5f; // Canavarın takip mesafesi
    public float attackDistance = 1.5f; // Canavarın saldırı mesafesi
    public int attackDamage = 1; // Canavarın vereceği hasar
    public float attackCooldown = 1f; // Saldırı soğuma süresi
    private float lastAttackTime = 0f; // Son saldırı zamanı

    private bool isFollowing = false; // Takip durumu
    private Animator animator; // Canavar animatörü
    private Rigidbody2D rb; // Canavarın Rigidbody2D bileşeni

    // Start is called before the first frame update
    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindWithTag("Player").transform; // Oyuncuyu bul
        }

        animator = GetComponent<Animator>(); // Animator referansını al
        rb = GetComponent<Rigidbody2D>(); // Rigidbody2D referansını al
    }

    // Update is called once per frame
    void Update()
    {
        // Canavar ile oyuncu arasındaki mesafeyi hesapla
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Oyuncuya yaklaşma durumu
        if (distanceToPlayer < followDistance)
        {
            isFollowing = true;
        }
        else
        {
            isFollowing = false;
        }

        // Takip etme ve saldırma işlemleri
        if (isFollowing)
        {
            FollowPlayer(); // Oyuncuyu takip et
        }

        // Oyuncuya saldırma
        if (distanceToPlayer <= attackDistance && Time.time > lastAttackTime + attackCooldown)
        {
            AttackPlayer(); // Oyuncuya saldır
        }
    }

    // Oyuncuyu takip etme fonksiyonu
    void FollowPlayer()
    {
        // Oyuncuya doğru yönel
        Vector2 direction = (player.position - transform.position).normalized;

        // Canavarı oyuncuya doğru hareket ettir
        transform.Translate(direction * moveSpeed * Time.deltaTime);
    }

    // Oyuncuya saldırma fonksiyonu
    void AttackPlayer()
    {
        lastAttackTime = Time.time; // Son saldırı zamanını güncelle

        // Oyuncuya hasar ver
        PlayerHP playerHP = player.GetComponent<PlayerHP>(); // PlayerHP script’ini al
        if (playerHP != null)
        {
            Vector2 knockbackDirection = new Vector2(player.position.x - transform.position.x, 0).normalized;
            playerHP.TakeDamage(attackDamage, knockbackDirection); // Oyuncuya hasar ver ve knockback uygula
        }

        // Saldırı animasyonunu tetikle
        animator.SetTrigger("Attack");
        Debug.Log("Canavar oyuncuya saldırdı!");
    }
}
