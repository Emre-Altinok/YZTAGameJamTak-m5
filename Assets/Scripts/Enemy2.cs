using System.Collections;
using UnityEngine;

public class Enemy2 : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Movement Settings")]
    public float moveSpeed = 2.5f;
    public float followDistance = 8f; // Takip mesafesi
    public float attackDistance = 1.5f; // Sald�r� mesafesi
    public Transform player; // Oyuncu referans�

    [Header("Attack Settings")]
    public int attackDamage = 1;
    public float attackCooldown = 2f; // Sald�r� bekleme s�resi
    private float lastAttackTime = 0f;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip walkSound; // Yürüme sesi
    [SerializeField] private AudioClip attackSound; // Saldırı sesi
    [SerializeField] private AudioClip deathSound; // Ölüm sesi
    [Range(0f, 1f)]
    [SerializeField] private float walkVolume = 0.5f; // Yürüme sesi seviyesi
    [Range(0f, 1f)]
    [SerializeField] private float attackVolume = 0.7f; // Saldırı sesi seviyesi
    [Range(0f, 1f)]
    [SerializeField] private float deathVolume = 0.8f; // Ölüm sesi seviyesi
    private AudioSource audioSource;

    [Header("Attack Hitbox")]
    public GameObject attackHitboxObject; // Sald�r� hitbox objesi
    private Collider2D attackCollider; // Sald�r� hitbox'�

    // Durum De�i�kenleri
    private bool isFollowing = false;
    private bool isAttacking = false;
    private bool isDead = false;

    // Bile�enler
    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Collider2D[] colliders;

    void Start()
    {
        // Bile�enleri al
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        colliders = GetComponents<Collider2D>();

        // AudioSource bile�enini al veya ekle
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Ba�lang�� de�erleri
        currentHealth = maxHealth;
        SetupAttackHitbox();

        // E�er player referans� atanmam��sa, Player tag'ine sahip objeyi bul
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
        // �nce Inspector'dan atama yap�lm�� m� kontrol et
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

        // Son kontrol ve haz�rl�k
        if (attackCollider != null)
        {
            // Sald�r� hitbox'�n� haz�rla - trigger oldu�undan emin ol
            attackCollider.isTrigger = true;
            attackCollider.enabled = false;
            Debug.Log("Attack hitbox setup complete: " + attackCollider.name);
        }
        else
        {
            Debug.LogWarning("AttackHitbox bulunamad�! D��man�n sald�r� collider'� �al��mayacak.");
        }
    }

    void Update()
    {
        // E�er d��man �lm��se veya oyuncu yoksa hi�bir �ey yapma
        if (isDead || player == null) return;

        // Oyuncuya olan mesafeyi hesapla
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Oyuncu �lm�� m� kontrol et
        PlayerHP playerHP = player.GetComponent<PlayerHP>();
        if (playerHP != null && playerHP.GetCurrentHealth() <= 0)
        {
            StopMovement();
            return;
        }

        // Takip ve sald�r� mant���
        if (distanceToPlayer <= followDistance && distanceToPlayer > attackDistance)
        {
            isFollowing = true;
            FollowPlayer();
        }
        else if (distanceToPlayer <= attackDistance && !isAttacking && Time.time > lastAttackTime + attackCooldown)
        {
            isFollowing = false;
            StopMovement();
            AttackPlayer();
        }
        else if (distanceToPlayer > followDistance)
        {
            isFollowing = false;
            StopMovement();
        }

        // Animasyon i�in h�z parametresini g�ncelle
        animator.SetFloat("Speed", isFollowing ? moveSpeed : 0f);
    }

    void FollowPlayer()
    {
        // Y�r�me sesini �al (e�er �alm�yorsa)
        if (!audioSource.isPlaying && walkSound != null)
        {
            audioSource.clip = walkSound;
            audioSource.volume = walkVolume;
            audioSource.loop = true;
            audioSource.Play();
        }

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

        // Hareketi Rigidbody2D ile yap
        rb.MovePosition(rb.position + direction * moveSpeed * Time.deltaTime);
    }

    void StopMovement()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // Y�r�me sesini durdur
        if (audioSource.isPlaying && audioSource.clip == walkSound)
        {
            audioSource.Stop();
        }
    }

    void AttackPlayer()
    {
        lastAttackTime = Time.time;

        // Sald�r� animasyonunu tetikle  
        animator.SetTrigger("Attack");

        // Sald�r� durumunu ayarla ve coroutine ba�lat
        isAttacking = true;
        StartCoroutine(HandleAttackSequence());
    }

    private IEnumerator HandleAttackSequence()
    {
        // Sald�r� sesi �al
        if (attackSound != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
            audioSource.clip = attackSound;
            audioSource.volume = attackVolume;
            audioSource.Play();
        }

        // �nce bir animasyon bilgisi alal�m
        AnimatorStateInfo animStateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // Animasyon ba�layana kadar k�sa bir bekleme
        float waitTime = 0;
        while (!animStateInfo.IsName("Attack") && waitTime < 0.5f) // G�venlik i�in 0.5 saniye limit
        {
            waitTime += Time.deltaTime;
            animStateInfo = animator.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }

        // Sald�r� ba�lang�c�
        if (attackCollider != null)
        {
            attackCollider.enabled = true;
        }

        // Animasyonun ortas�na kadar bekle ve hasar ver
        while (animStateInfo.IsName("Attack") && animStateInfo.normalizedTime < 0.5f)
        {
            animStateInfo = animator.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }

        // Hasar verme an�
        TryDamagePlayer();

        // Animasyonun %80'ine kadar bekle
        while (animStateInfo.IsName("Attack") && animStateInfo.normalizedTime < 0.8f)
        {
            animStateInfo = animator.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }

        // Hitbox'� devre d��� b�rak
        if (attackCollider != null)
        {
            attackCollider.enabled = false;
        }

        // Animasyonun tamamlanmas�n� bekle
        while (animStateInfo.IsName("Attack") && animStateInfo.normalizedTime < 1.0f)
        {
            animStateInfo = animator.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }

        // Sald�r� durumunu s�f�rla
        isAttacking = false;
    }

    private void TryDamagePlayer()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackDistance)
        {
            PlayerHP playerHP = player.GetComponent<PlayerHP>();
            if (playerHP != null)
            {
                // D��mandan oyuncuya do�ru knockback y�n� hesapla
                Vector2 knockbackDirection = (player.position - transform.position).normalized;
                playerHP.TakeDamage(attackDamage, knockbackDirection);
                Debug.Log("Enemy2 attacked the player! Damage: " + attackDamage);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log("Enemy2 took " + damage + " damage! Remaining health: " + currentHealth);

        // Hasar alma animasyonunu tetikle
        animator.SetTrigger("Hurt");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        Debug.Log("Enemy2 is dead!");

        // Tüm aktif Coroutine'leri durdur
        StopAllCoroutines();

        // Hareket ve saldırı durumlarını sıfırla
        isFollowing = false;
        isAttacking = false;

        // Mevcut sesi durdur
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        // Ölüm sesini çal
        if (deathSound != null)
        {
            audioSource.loop = false;
            audioSource.clip = deathSound;
            audioSource.volume = deathVolume;
            audioSource.Play();
        }

        // Ölüm animasyonunu tetikle
        animator.SetTrigger("Death");

        // Rigidbody2D ve Collider2D bileşenlerini devre dışı bırak
        rb.simulated = false;
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }

        // Ölüm animasyonunun tamamlanmasını bekle
        StartCoroutine(WaitForDeathAnimation());
    }

    private IEnumerator WaitForDeathAnimation()
    {
        // �l�m animasyonunun tamamlanmas�n� bekle
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        while (!stateInfo.IsName("Death") || stateInfo.normalizedTime < 1.0f)
        {
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }

        // Animat�r� devre d��� b�rak
        animator.enabled = false;

        // GameObject'i yok et
        Destroy(gameObject, 0.5f);
    }

    // Mermi ile �arp��ma alg�lama
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            // Bullet script'inden hasar de�erini al
            Bullet bullet = other.GetComponent<Bullet>();
            if (bullet != null)
            {
                TakeDamage(bullet.damage);
            }
            else
            {
                // Bullet script'i yoksa varsay�lan 1 hasar ver
                TakeDamage(1);
            }

            // Mermiyi yok et
            Destroy(other.gameObject);
        }
    }
}
