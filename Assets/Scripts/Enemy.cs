using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

// UnityEditor namespace'ini kaldırdım - build hatalarını önler
public class Enemy : MonoBehaviour
{
    public int health = 2;
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
    private Collider2D[] colliders; // Tüm çarpışma bileşenleri  
    private SpriteRenderer spriteRenderer; // SpriteRenderer referansı  

    // Inspector üzerinden atama yapılabilmesi için public
    [Header("Attack Settings")]
    public GameObject attackHitboxObject; // Saldırı Hitbox objesi
    private Collider2D attackCollider; // Saldırı hitboxı için
    private bool isAttacking = false; // Saldırı durumu

    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindWithTag("Player").transform;
        }

        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        colliders = GetComponents<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        SetupAttackHitbox();
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
            Transform attackHitboxTransform = transform.Find("AttackHitbox");
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
        // Eğer düşman ölüyse hiçbir şey yapma
        if (animator.GetBool("isDead"))
        {
            return;
        }

        // Eğer player null ise bir şey yapma
        if (player == null) return;

        // Eğer oyuncu ölmüşse düşman animasyonlarını durdur
        PlayerHP playerHP = player.GetComponent<PlayerHP>();
        if (playerHP != null && playerHP.GetCurrentHealth() <= 0)
        {
            animator.SetFloat("Speed", 0f); // Hızı sıfırla
            animator.SetBool("isDead", false); // Diğer animasyonları durdur
            return; // Takip ve saldırı işlemlerini durdur
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Oyuncuya yaklaşma ve saldırma mantığı
        if (distanceToPlayer <= followDistance && distanceToPlayer > attackDistance)
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
        else if (distanceToPlayer <= attackDistance && !isAttacking && Time.time > lastAttackTime + attackCooldown)
        {
            AttackPlayer();
        }

        // Animasyon için hız parametresini güncelle
        animator.SetFloat("Speed", isFollowing ? moveSpeed : 0f);
    }

    void FollowPlayer()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Eğer oyuncu saldırı mesafesine girdiyse hareketi durdur
        if (distanceToPlayer <= attackDistance)
        {
            rb.linearVelocity = Vector2.zero; // Hareketi durdur
            return;
        }

        Vector2 direction = (player.position - transform.position).normalized;

        // Sprite'ı hareket yönüne göre çevir
        if (direction.x > 0)
        {
            spriteRenderer.flipX = false; // Sağ tarafa bak
        }
        else if (direction.x < 0)
        {
            spriteRenderer.flipX = true; // Sol tarafa bak
        }

        // Hareketi Rigidbody2D ile yap
        rb.MovePosition(rb.position + direction * moveSpeed * Time.deltaTime);
    }

    void AttackPlayer()
    {
        lastAttackTime = Time.time;

        // Saldırı animasyonunu tetikle  
        animator.SetTrigger("Attack");

        // Saldırı durumunu ayarla ve coroutine başlat
        isAttacking = true;
        StartCoroutine(HandleAttackSequence());
    }

    // Saldırı sıralamasını daha ayrıntılı yönet
    private IEnumerator HandleAttackSequence()
    {
        // Önce bir animasyon bilgisi alalım
        AnimatorStateInfo animStateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // Animasyon başlayana kadar kısa bir bekleme
        float waitTime = 0;
        while (!animStateInfo.IsName("Attack") && waitTime < 0.5f) // Güvenlik için 0.5 saniye limit
        {
            waitTime += Time.deltaTime;
            animStateInfo = animator.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }

        // Saldırı başlangıcı - animasyonun yaklaşık %20'sine karşılık gelebilir
        if (attackCollider != null)
        {
            attackCollider.enabled = true;
            Debug.Log("Attack hitbox activated at animation time: " + animStateInfo.normalizedTime);
        }

        // Animasyonun ortasına kadar bekle (yaklaşık %50'si) ve hasar ver
        while (animStateInfo.IsName("Attack") && animStateInfo.normalizedTime < 0.5f)
        {
            animStateInfo = animator.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }

        // Hasar verme anı - animasyonun ortasında
        TryDamagePlayer();

        // Animasyonun yaklaşık %80'ine kadar bekle
        while (animStateInfo.IsName("Attack") && animStateInfo.normalizedTime < 0.8f)
        {
            animStateInfo = animator.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }

        // Hitbox'ı devre dışı bırak
        if (attackCollider != null)
        {
            attackCollider.enabled = false;
            Debug.Log("Attack hitbox deactivated at animation time: " + animStateInfo.normalizedTime);
        }

        // Animasyonun tamamlanmasını bekle
        while (animStateInfo.IsName("Attack") && animStateInfo.normalizedTime < 1.0f)
        {
            animStateInfo = animator.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }

        // Saldırı durumunu sıfırla
        isAttacking = false;
    }

    // Hasar verme işlemini düzeltilmiş metod
    private void TryDamagePlayer()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackDistance)
        {
            PlayerHP playerHP = player.GetComponent<PlayerHP>();
            if (playerHP != null)
            {
                // Knockback olmadan hasar ver
                // Vector2.zero knockback yönü olarak geçiyoruz, böylece knockback uygulanmaz
                playerHP.TakeDamage(attackDamage, Vector2.zero);
                Debug.Log("Enemy attacked the player! Damage: " + attackDamage);
            }
        }
    }

    public void TakeDamage(int damage, Vector2 knockbackDirection)
    {
        health -= damage;
        Debug.Log("Enemy took " + damage + " damage! Remaining health: " + health);

        // Knockback uygula
        if (rb != null)
        {
            // Mevcut hareketi sıfırla
            rb.linearVelocity = Vector2.zero;

            // Knockback kuvveti uygula (Y ekseninde kuvveti sınırla)
            Vector2 limitedKnockback = knockbackDirection;
            rb.AddForce(limitedKnockback * 3f, ForceMode2D.Impulse);

            // Knockback sonrası yerçekimini yönetmek için coroutine
            StartCoroutine(ResetVelocityAfterKnockback());

            Debug.Log("Enemy knocked back!");
        }

        if (health <= 0)
        {
            Die();
        }
    }
    // Knockback sonrası velocity'yi sıfırlamak için coroutine
    private IEnumerator ResetVelocityAfterKnockback()
    {
        // Kısa bir süre bekle
        yield return new WaitForSeconds(0.2f);

        // Y eksenindeki hızı sıfırla (düşmanın havada takılmasını engelle)
        if (rb != null)
        {
            Vector2 currentVelocity = rb.linearVelocity;
            rb.linearVelocity = new Vector2(currentVelocity.x * 0.5f, 0);
        }
    }
    void Die()
    {
        Debug.Log("Enemy is dead!");

        // Tüm aktif Coroutine'leri durdur
        StopAllCoroutines();

        // Saldırı durumunu sıfırla
        isAttacking = false;
        if (attackCollider != null)
        {
            attackCollider.enabled = false;
        }

        // Ölüm animasyonunu tetikle  
        animator.SetBool("isDead", true);

        // Rigidbody2D ve Collider2D bileşenlerini devre dışı bırak  
        rb.simulated = false; // Rigidbody2D'yi devre dışı bırak  
        foreach (var collider in colliders)
        {
            collider.enabled = false; // Tüm Collider2D bileşenlerini devre dışı bırak  
        }

        // Ölüm animasyonunun tamamlanmasını bekle  
        StartCoroutine(WaitForDeathAnimation());
    }

    private IEnumerator WaitForDeathAnimation()
    {
        // Ölüm animasyonunun tamamlanmasını bekle  
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        while (!stateInfo.IsName("Death") || stateInfo.normalizedTime < 1.0f)
        {
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }

        // Animatörü devre dışı bırak  
        animator.enabled = false;

        // GameObject'i yok et
        Destroy(gameObject);
    }
}
