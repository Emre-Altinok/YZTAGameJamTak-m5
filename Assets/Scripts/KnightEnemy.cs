using System.Collections;
using UnityEngine;

public class KnightEnemy : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float followDistance = 10f; // Takip mesafesi
    public float attackDistance = 2f; // Saldırı mesafesi
    public float minDistanceToKeep = 1.5f; // Korunacak minimum mesafe
    public Transform player; // Oyuncu referansı

    [Header("Attack Settings")]
    public int attackDamage = 10;
    public float attackCooldown = 2f; // Saldırı bekleme süresi
    private float lastAttackTime = 0f;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip walkSound; // Yürüme sesi
    [SerializeField] private AudioClip attackSound; // Saldırı sesi
    [SerializeField] private AudioClip hurtSound; // Hasar alma sesi
    [SerializeField] private AudioClip deathSound; // Ölüm sesi
    [Range(0f, 1f)]
    [SerializeField] private float walkVolume = 0.5f;
    [Range(0f, 1f)]
    [SerializeField] private float attackVolume = 0.7f;
    [Range(0f, 1f)]
    [SerializeField] private float hurtVolume = 0.6f;
    [Range(0f, 1f)]
    [SerializeField] private float deathVolume = 0.8f;
    private AudioSource audioSource;

    [Header("Attack Hitbox")]
    public GameObject attackHitboxObject; // Saldırı hitbox objesi
    private Collider2D attackCollider; // Saldırı hitbox'ı

    // Durum Değişkenleri
    private bool isFollowing = false;
    private bool isAttacking = false;
    private bool isDead = false;
    private bool isFacingRight = true;

    // Bileşenler
    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Collider2D[] colliders;

    void Start()
    {
        // Bileşenleri al
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        colliders = GetComponents<Collider2D>();

        // AudioSource bileşenini al veya ekle
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Başlangıç değerleri
        currentHealth = maxHealth;
        SetupAttackHitbox();

        // Eğer player referansı atanmamışsa, Player tag'ine sahip objeyi bul
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }

    void SetupAttackHitbox()
    {
        // Önce Inspector'dan atama yapılmış mı kontrol et
        if (attackHitboxObject != null)
        {
            attackCollider = attackHitboxObject.GetComponent<Collider2D>();
        }
        else
        {
            // Transformdan bul
            Transform attackHitboxTransform = transform.Find("SwordHitbox");
            if (attackHitboxTransform != null)
            {
                attackHitboxObject = attackHitboxTransform.gameObject;
                attackCollider = attackHitboxObject.GetComponent<Collider2D>();
            }
        }

        // Son kontrol ve hazırlık
        if (attackCollider != null)
        {
            // Saldırı hitbox'ını hazırla - trigger olduğundan emin ol
            attackCollider.isTrigger = true;
            attackCollider.enabled = false;
            Debug.Log("Attack hitbox setup complete: " + attackCollider.name);
        }
        else
        {
            Debug.LogWarning("AttackHitbox bulunamadı! Düşmanın saldırı collider'ı çalışmayacak.");
        }
    }

    void Update()
    {
        // Eğer düşman ölmüşse veya oyuncu yoksa hiçbir şey yapma
        if (isDead || player == null) return;

        // Oyuncuya olan mesafeyi hesapla
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Oyuncu ölmüş mü kontrol et
        PlayerHP playerHP = player.GetComponent<PlayerHP>();
        if (playerHP != null && playerHP.GetCurrentHealth() <= 0)
        {
            StopMovement();
            return;
        }

        // Takip ve saldırı mantığı
        if (distanceToPlayer <= followDistance)
        {
            // Yüzün oyuncuya doğru
            FacePlayer();

            if (distanceToPlayer > attackDistance && distanceToPlayer > minDistanceToKeep)
            {
                // Oyuncuya doğru hareket et
                isFollowing = true;
                MoveTowardsPlayer();
            }
            else if (distanceToPlayer < minDistanceToKeep)
            {
                // Oyuncudan uzaklaş
                isFollowing = true;
                MoveAwayFromPlayer();
            }
            else if (distanceToPlayer <= attackDistance && !isAttacking && Time.time > lastAttackTime + attackCooldown)
            {
                // Saldır
                isFollowing = false;
                StopMovement();
                AttackPlayer();
            }
            else
            {
                // Durma
                isFollowing = false;
                StopMovement();
            }
        }
        else
        {
            // Algılama menzili dışında, dur
            isFollowing = false;
            StopMovement();
        }

        // Animasyon için hız parametresini güncelle
        if (animator != null && !isDead)
        {
            animator.SetFloat("Speed", isFollowing ? moveSpeed : 0f);
        }
    }

    void FacePlayer()
    {
        if (player == null) return;

        // Oyuncunun konumuna göre döndürme
        bool shouldFaceRight = player.position.x > transform.position.x;

        if (shouldFaceRight != isFacingRight)
        {
            // Karakter yönünü çevir
            Flip();
        }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void MoveTowardsPlayer()
    {
        if (rb == null || player == null) return;

        // Yürüme sesini çal (eğer çalmıyorsa)
        if (!audioSource.isPlaying && walkSound != null)
        {
            audioSource.clip = walkSound;
            audioSource.volume = walkVolume;
            audioSource.loop = true;
            audioSource.Play();
        }

        Vector2 direction = (player.position - transform.position).normalized;

        // X yönünde hareketi koru, Y yönünde yerçekimi etkisini muhafaza et
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);
    }

    void MoveAwayFromPlayer()
    {
        if (rb == null || player == null) return;

        // Yürüme sesini çal (eğer çalmıyorsa)
        if (!audioSource.isPlaying && walkSound != null)
        {
            audioSource.clip = walkSound;
            audioSource.volume = walkVolume;
            audioSource.loop = true;
            audioSource.Play();
        }

        Vector2 direction = (transform.position - player.position).normalized;

        // X yönünde hareketi koru, Y yönünde yerçekimi etkisini muhafaza et
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);
    }

    void StopMovement()
    {
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // Y hızını koru
        }

        // Yürüme sesini durdur
        if (audioSource != null && audioSource.isPlaying && audioSource.clip == walkSound)
        {
            audioSource.Stop();
        }
    }

    void AttackPlayer()
    {
        if (animator == null || isDead) return;

        lastAttackTime = Time.time;

        // Saldırı animasyonunu tetikle
        animator.SetTrigger("Attack");

        // Saldırı durumunu ayarla ve coroutine başlat
        isAttacking = true;
        StartCoroutine(HandleAttackSequence());
    }

    private IEnumerator HandleAttackSequence()
    {
        // Saldırı sesi çal
        if (attackSound != null && audioSource != null && !isDead)
        {
            audioSource.Stop();
            audioSource.loop = false;
            audioSource.clip = attackSound;
            audioSource.volume = attackVolume;
            audioSource.Play();
        }

        // Ölüm kontrolü
        if (isDead)
        {
            isAttacking = false;
            yield break; // Coroutine'i hemen sonlandır
        }

        // Animasyon bilgisi
        AnimatorStateInfo animStateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // Animasyon başlayana kadar kısa bir bekleme
        float waitTime = 0;
        while (!animStateInfo.IsName("Attack") && waitTime < 0.5f && !isDead)
        {
            waitTime += Time.deltaTime;
            animStateInfo = animator.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }

        // Ölüm kontrolü
        if (isDead)
        {
            isAttacking = false;
            yield break;
        }

        // Saldırı başlangıcı
        if (attackCollider != null)
        {
            attackCollider.enabled = true;
        }

        // Animasyonun ortasına kadar bekle ve hasar ver
        waitTime = 0;
        while (animStateInfo.IsName("Attack") && animStateInfo.normalizedTime < 0.5f && waitTime < 1f && !isDead)
        {
            waitTime += Time.deltaTime;
            animStateInfo = animator.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }

        // Ölüm kontrolü
        if (isDead)
        {
            // Hitbox'ı devre dışı bırak
            if (attackCollider != null)
            {
                attackCollider.enabled = false;
            }
            isAttacking = false;
            yield break;
        }

        // Hasar verme anı
        TryDamagePlayer();

        // Animasyonun %80'ine kadar bekle
        waitTime = 0;
        while (animStateInfo.IsName("Attack") && animStateInfo.normalizedTime < 0.8f && waitTime < 1f && !isDead)
        {
            waitTime += Time.deltaTime;
            animStateInfo = animator.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }

        // Hitbox'ı devre dışı bırak
        if (attackCollider != null)
        {
            attackCollider.enabled = false;
        }

        // Ölüm kontrolü
        if (isDead)
        {
            isAttacking = false;
            yield break;
        }

        // Animasyonun tamamlanmasını bekle (zaman aşımı güvenliği ile)
        waitTime = 0;
        while (animStateInfo.IsName("Attack") && animStateInfo.normalizedTime < 1.0f && waitTime < 1.5f && !isDead)
        {
            waitTime += Time.deltaTime;
            animStateInfo = animator.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }

        // Saldırı durumunu sıfırla
        isAttacking = false;
    }

    private void TryDamagePlayer()
    {
        if (player == null || isDead) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackDistance)
        {
            PlayerHP playerHP = player.GetComponent<PlayerHP>();
            if (playerHP != null)
            {
                // Düşmandan oyuncuya doğru knockback yönü hesapla
                Vector2 knockbackDirection = (player.position - transform.position).normalized;
                playerHP.TakeDamage(attackDamage, knockbackDirection);
                Debug.Log("KnightEnemy attacked the player! Damage: " + attackDamage);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log("KnightEnemy took " + damage + " damage! Remaining health: " + currentHealth);

        // Hasar alma sesi çal
        if (hurtSound != null && audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
            audioSource.clip = hurtSound;
            audioSource.volume = hurtVolume;
            audioSource.Play();
        }

        // Hasar alma animasyonunu tetikle
        if (animator != null && !isDead)
        {
            animator.SetTrigger("Hurt");
        }

        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
    }

    void Die()
    {
        // Ölüm durumunu önce ayarla
        isDead = true;
        Debug.Log("KnightEnemy is dead!");

        // Tüm aktif Coroutine'leri durdur
        StopAllCoroutines();

        // Hareket ve saldırı durumlarını sıfırla
        isFollowing = false;
        isAttacking = false;

        // Mevcut sesi durdur
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        // Ölüm sesini çal
        if (deathSound != null && audioSource != null)
        {
            audioSource.loop = false;
            audioSource.clip = deathSound;
            audioSource.volume = deathVolume;
            audioSource.Play();
        }

        // Ölüm animasyonunu tetikle
        if (animator != null)
        {
            // Diğer tüm trigger'ları sıfırla
            animator.ResetTrigger("Attack");
            animator.ResetTrigger("Hurt");
            animator.SetTrigger("Death");

            // Durma durumuna ayarla
            animator.SetFloat("Speed", 0);
        }

        // Fizik ve çarpışmaları devre dışı bırak
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        if (colliders != null)
        {
            foreach (var collider in colliders)
            {
                if (collider != null)
                {
                    collider.enabled = false;
                }
            }
        }

        // Attack hitbox'ı devre dışı bırak
        if (attackCollider != null)
        {
            attackCollider.enabled = false;
        }

        // Doğrudan yok etme zamanlaması ayarla (animasyon beklemeden)
        Destroy(gameObject, 2f);
    }

    // KnightController'ın kılıç darbesi için
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        // KnightController'ın SwordHitbox'ı ile çarpışma kontrolü
        if (other.CompareTag("PlayerWeapon"))
        {
            // Kılıç bileşeninden hasar değerini almaya çalış
            KnightSwordHitbox swordHitbox = other.GetComponent<KnightSwordHitbox>();
            int damageAmount = 10; // Varsayılan hasar

            if (swordHitbox != null)
            {
                damageAmount = swordHitbox.damage;
            }

            // Hasarı uygula
            TakeDamage(damageAmount);
        }
    }

    // Gizmos ile görselleştirme
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, followDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, minDistanceToKeep);
    }
}

// SwordHitbox bileşeni, düşmanın kılıcı için
public class KnightEnemySwordHitbox : MonoBehaviour
{
    [SerializeField] private int damage = 10;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Oyuncuya hasar ver
            PlayerHP playerHP = collision.GetComponent<PlayerHP>();
            if (playerHP != null)
            {
                // Knockback yönünü hesapla
                Vector2 knockbackDirection = (collision.transform.position - transform.parent.position).normalized;
                playerHP.TakeDamage(damage, knockbackDirection);
                Debug.Log("Knight enemy sword hit the player!");
            }
        }
    }
}

