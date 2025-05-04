using UnityEngine;

public class KnightEnemy : MonoBehaviour
{
    // Sa�l�k Parametreleri
    [SerializeField] private int health = 100;

    // Hareket Parametreleri
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float minDistanceToKeep = 1.5f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private int attackDamage = 10;

    // Bile�enler
    private Animator animator;
    private Rigidbody2D rb;
    private Transform playerTransform;
    private GameObject swordHitbox;

    // Durum de�i�kenleri
    private bool canAttack = true;
    private float lastAttackTime;
    private bool isDead = false;
    private bool isFacingRight = true;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // Sword Hitbox'� bul
        swordHitbox = transform.Find("SwordHitbox")?.gameObject;
        if (swordHitbox != null)
        {
            swordHitbox.SetActive(false);
        }
    }

    private void Start()
    {
        // Oyuncuyu bul
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform == null)
        {
            Debug.LogError("KnightEnemy: Player with tag 'Player' not found!");
        }
    }

    private void Update()
    {
        if (isDead || playerTransform == null) return;

        // Oyuncuya olan mesafe
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // Oyuncu alg�lama menzilinde mi?
        if (distanceToPlayer <= detectionRange)
        {
            // Y�z�n oyuncuya do�ru
            FacePlayer();

            // Oyuncu sald�r� menzilinde mi?
            if (distanceToPlayer <= attackRange && canAttack)
            {
                // Sald�r
                Attack();
            }
            else if (distanceToPlayer > minDistanceToKeep)
            {
                // Oyuncuya do�ru hareket et
                MoveTowardsPlayer();
            }
            else if (distanceToPlayer < minDistanceToKeep)
            {
                // Oyuncudan uzakla�
                MoveAwayFromPlayer();
            }
            else
            {
                // Durma pozisyonu
                StopMoving();
            }
        }
        else
        {
            // Alg�lama menzili d���nda, dur
            StopMoving();
        }

        // Sald�r� cooldown kontrol�
        if (!canAttack && Time.time > lastAttackTime + attackCooldown)
        {
            canAttack = true;
        }
    }

    private void MoveTowardsPlayer()
    {
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);

        // Hareket animasyonu
        animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
    }

    private void MoveAwayFromPlayer()
    {
        Vector2 direction = (transform.position - playerTransform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);

        // Hareket animasyonu
        animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
    }

    private void StopMoving()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        // Durma animasyonu
        animator.SetFloat("Speed", 0);
    }

    private void FacePlayer()
    {
        // Oyuncunun konumuna g�re d�nd�rme
        bool shouldFaceRight = playerTransform.position.x > transform.position.x;

        if (shouldFaceRight != isFacingRight)
        {
            Flip();
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void Attack()
    {
        // Sald�r� ba�latma
        animator.SetTrigger("Attack");
        canAttack = false;
        lastAttackTime = Time.time;

        // K�l�� hitbox'�n� aktifle�tir
        if (swordHitbox != null)
        {
            swordHitbox.SetActive(true);
            Invoke(nameof(DisableSwordHitbox), 0.5f);
        }
        else
        {
            // K�l�� hitbox yoksa, do�rudan oyuncuya hasar ver
            ApplyDamageToPlayer();
        }
    }

    private void DisableSwordHitbox()
    {
        if (swordHitbox != null)
        {
            swordHitbox.SetActive(false);
        }
    }

    private void ApplyDamageToPlayer()
    {
        // Oyuncunun menzilde olup olmad���n� kontrol et
        if (Vector2.Distance(transform.position, playerTransform.position) <= attackRange)
        {
            // Knockback y�n�n� hesapla
            Vector2 knockbackDirection = (playerTransform.position - transform.position).normalized;

            // Oyuncuya hasar ver
            PlayerHP playerHP = playerTransform.GetComponent<PlayerHP>();
            if (playerHP != null)
            {
                playerHP.TakeDamage(attackDamage, knockbackDirection);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        health -= damage;

        // Hasar animasyonu tetikle
        animator.SetTrigger("Hurt");

        // E�er can 0 veya alt�na d��erse �l�m animasyonu tetikle
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        // Fizik ve �arp��malar� devre d��� b�rak
        rb.linearVelocity = Vector2.zero;
        rb.isKinematic = true;

        // Collider'� devre d��� b�rak
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        // �l�m animasyonu tetikle
        animator.SetTrigger("Death");

        // D��man� devre d��� b�rak
        Destroy(gameObject, 1f); // 1 saniye sonra d��man� yok et
    }

    // Gizmos ile g�rselle�tirme
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, minDistanceToKeep);
    }
}

// SwordHitbox bile�eni, d��man�n k�l�c� i�in
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
                // Knockback y�n�n� hesapla
                Vector2 knockbackDirection = (collision.transform.position - transform.parent.position).normalized;
                playerHP.TakeDamage(damage, knockbackDirection);
            }
        }
    }
}
