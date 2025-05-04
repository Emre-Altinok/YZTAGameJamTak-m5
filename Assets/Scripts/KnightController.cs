using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class KnightController : MonoBehaviour
{
    // Bileşen referansları
    private Animator animator;
    private Rigidbody2D rb;
    private PlayerHP playerHP;
    private bool isDead = false;

    // Hareket parametreleri
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    private bool isGrounded;

    // Ses parametreleri
    [Header("Audio Settings")]
    [SerializeField] private AudioClip walkingSound;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip hurtSound;
    [Range(0f, 1f)]
    [SerializeField] private float walkVolume = 0.7f;
    [Range(0f, 1f)]
    [SerializeField] private float attackVolume = 0.7f;
    [Range(0f, 1f)]
    [SerializeField] private float jumpVolume = 0.7f;
    [Range(0f, 1f)]
    [SerializeField] private float hurtVolume = 0.7f;
    private AudioSource walkAudioSource;
    private AudioSource effectsAudioSource;

    // Saldırı parametreleri
    [Header("Attack Settings")]
    [SerializeField] private GameObject swordHitbox;
    [SerializeField] private float attackCooldown = 0.5f;
    [Tooltip("Oyuncunun düşmanlara vereceği hasar miktarı")]
    [SerializeField] private int attackDamage = 10;
    private float lastAttackTime = 0f;
    private KnightSwordHitbox swordHitboxComponent;

    // Input System
    [Header("Input System")]
    [SerializeField] private InputActionAsset inputActions;
    private InputAction moveAction, jumpAction, attackAction, deathAction;

    // Animasyon parametreleri sabitleri
    private const string ANIM_IS_GROUNDED = "isGrounded";
    private const string ANIM_SPEED = "Speed";
    private const string ANIM_JUMP = "Jump";
    private const string ANIM_ATTACK = "Attack";
    private const string ANIM_HURT = "Hurt";
  //  private const string ANIM_DEATH = "Death";
    // Animatör parametrelerini güncelle
    private const string ANIM_IS_DEAD = "IsDead"; // Bool parametresi


    #region Unity Lifecycle Methods

    private void Awake()
    {
        SetupInputActions();
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        playerHP = GetComponent<PlayerHP>();

        // AudioSource bileşenlerini ayarla
        SetupAudioSources();

        if (animator != null)
        {
            ResetAnimatorParameters();
        }
        else
        {
            Debug.LogError("Animator component not found on knight!");
        }

        // SwordHitbox'ı başlangıçta devre dışı bırak
        if (swordHitbox != null)
        {
            // SwordHitbox bileşenini al veya ekle
            swordHitboxComponent = swordHitbox.GetComponent<KnightSwordHitbox>();
            if (swordHitboxComponent == null)
            {
                swordHitboxComponent = swordHitbox.AddComponent<KnightSwordHitbox>();
            }

            // Hasar değerini ayarla
            swordHitboxComponent.damage = attackDamage;

            // Tag'i ayarla (düşman tarafından algılanması için)
            swordHitbox.tag = "PlayerWeapon";

            // Başlangıçta devre dışı bırak
            swordHitbox.SetActive(false);
        }
        else
        {
            Debug.LogWarning("SwordHitbox not assigned, melee attacks will not work!");
        }
    }


    private void SetupAudioSources()
    {
        // Yürüme için AudioSource
        walkAudioSource = GetComponent<AudioSource>();
        if (walkAudioSource == null)
        {
            walkAudioSource = gameObject.AddComponent<AudioSource>();
        }

        // Yürüme sesi ayarları
        if (walkingSound != null)
        {
            walkAudioSource.clip = walkingSound;
            walkAudioSource.loop = true;
            walkAudioSource.playOnAwake = false;
            walkAudioSource.volume = walkVolume;
        }

        // Efektler için ikinci AudioSource
        effectsAudioSource = gameObject.AddComponent<AudioSource>();
        effectsAudioSource.loop = false;
        effectsAudioSource.playOnAwake = false;
    }

    private void OnEnable()
    {
        EnableInputActions();
    }

    private void OnDisable()
    {
        if (inputActions != null)
        {
            DisableInputActions();
        }
        else
        {
            Debug.LogWarning("InputActions is null in OnDisable.");
        }
    }

    private void Update()
    {
        // Oyuncu ölmüşse hareket etmesin
        if (!isDead)
        {
            MoveCharacter();
        }
    }

    #endregion

    #region Input System

    private void SetupInputActions()
    {
        if (inputActions == null)
        {
            Debug.LogError("Input Actions asset is not assigned!");
            return;
        }
        moveAction = inputActions.FindAction("Move");
        jumpAction = inputActions.FindAction("Jump");
        attackAction = inputActions.FindAction("Attack");
        deathAction = inputActions.FindAction("Death");
    }

    private void EnableInputActions()
    {
        if (inputActions == null) return;

        if (moveAction != null) moveAction.Enable();

        if (jumpAction != null)
        {
            jumpAction.Enable();
            jumpAction.performed += OnJump;
        }

        if (attackAction != null)
        {
            attackAction.Enable();
            attackAction.performed += OnAttack;
        }

        if (deathAction != null)
        {
            deathAction.Enable();
            deathAction.performed += OnDeath;
        }
    }

    public void DisableInputActions()
    {
        if (inputActions == null) return;

        if (moveAction != null) moveAction.Disable();

        if (jumpAction != null)
        {
            jumpAction.performed -= OnJump;
            jumpAction.Disable();
        }

        if (attackAction != null)
        {
            attackAction.performed -= OnAttack;
            attackAction.Disable();
        }

        if (deathAction != null)
        {
            deathAction.performed -= OnDeath;
            deathAction.Disable();
        }
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded && !isDead)
        {
            Jump();
        }
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed && !isDead)
        {
            Attack();
        }
    }

    private void OnDeath(InputAction.CallbackContext context)
    {
        if (context.performed && !isDead)
        {
            SetDeathState();
        }
    }

    // Dışarıdan ölüm tetiklenebilmesi için public metod
    public void SetDeathState()
    {
        if (isDead) return; // Eğer zaten ölüyse çift çağrıyı engelle

        isDead = true;
        Debug.Log("Knight öldü! SetDeathState çalıştı.");

        // Tüm diğer animasyon işlemlerini sıfırla
        if (animator != null)
        {
            // Tüm trigger'ları sıfırla
            animator.ResetTrigger(ANIM_JUMP);
            animator.ResetTrigger(ANIM_ATTACK);
            animator.ResetTrigger(ANIM_HURT);

            // Hareket parametrelerini sıfırla
            animator.SetBool(ANIM_IS_GROUNDED, true);
            animator.SetFloat(ANIM_SPEED, 0);

            // Ölüm animasyonunu tetikle - bool parametresi kullanılıyor
            animator.SetBool(ANIM_IS_DEAD, true);

            // Farklı yöntemler dene, eğer yukarıdaki çalışmazsa
            try
            {
                // Direkt animasyonu oynatmayı dene
                animator.Play("Death", 0, 0f);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Ölüm animasyonu oynatılamadı: {e.Message}");
            }
        }

        // Hareket etmeyi durdur ve fizik simülasyonunu devre dışı bırak
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        // Input aksiyonlarını devre dışı bırak
        DisableInputActions();

        // Sesleri durdur
        if (walkAudioSource != null && walkAudioSource.isPlaying) walkAudioSource.Stop();
        if (effectsAudioSource != null && effectsAudioSource.isPlaying) effectsAudioSource.Stop();

        // SwordHitbox'ı devre dışı bırak
        if (swordHitbox != null)
        {
            swordHitbox.SetActive(false);
        }

        // Tüm collider'ları devre dışı bırak
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (var collider in colliders)
        {
            if (collider != null) collider.enabled = false;
        }

        // Görsel olarak ölü olduğunu göster (eğer animasyon çalışmazsa)
        transform.eulerAngles = new Vector3(0, 0, 90); // Karakteri yan yatır

        // Bu scripti devre dışı bırak (ancak animasyon kontrollerini kaybetmemek için son olarak yap)
        StartCoroutine(DisableScriptAfterDelay(1f));
    }


    private IEnumerator DisableScriptAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Animator'ı devre dışı bırak
        if (animator != null)
        {
            animator.enabled = false;
        }

        // Bu scripti devre dışı bırak
        enabled = false;
    }


    #endregion

    #region Character Control Methods

    private void ResetAnimatorParameters()
    {
        if (animator == null) return;

        animator.SetBool(ANIM_IS_GROUNDED, true);
        animator.SetFloat(ANIM_SPEED, 0);

        animator.ResetTrigger(ANIM_JUMP);
        animator.ResetTrigger(ANIM_ATTACK);
        animator.ResetTrigger(ANIM_HURT);
        animator.ResetTrigger(ANIM_IS_DEAD);
    }

    private void MoveCharacter()
    {
        if (moveAction == null || animator == null) return;

        float horizontal = moveAction.ReadValue<Vector2>().x;

        animator.SetFloat(ANIM_SPEED, Mathf.Abs(horizontal));

        if (horizontal != 0)
        {
            // Karakter hareket ediyor
            transform.Translate(new Vector2(horizontal * moveSpeed * Time.deltaTime, 0));
            transform.localScale = new Vector3(horizontal > 0 ? 1 : -1, 1, 1);

            // Yürüme sesini çal
            if (!walkAudioSource.isPlaying && isGrounded)
            {
                walkAudioSource.Play();
            }
        }
        else
        {
            // Karakter duruyor
            if (walkAudioSource.isPlaying)
            {
                walkAudioSource.Stop();
            }
        }
    }

    private void Jump()
    {
        if (rb == null || animator == null || !isGrounded) return;

        isGrounded = false;
        animator.SetTrigger(ANIM_JUMP);
        animator.SetBool(ANIM_IS_GROUNDED, false);

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        // Zıplama sesini çal
        PlaySound(jumpSound, jumpVolume);

        // Zıplarken yürüme sesini durdur
        if (walkAudioSource.isPlaying)
        {
            walkAudioSource.Stop();
        }
    }

    private void Attack()
    {
        // Saldırı hızı kontrolü
        if (Time.time < lastAttackTime + attackCooldown) return;

        lastAttackTime = Time.time;

        // Saldırı animasyonunu tetikle
        if (animator != null) animator.SetTrigger(ANIM_ATTACK);

        // Saldırı sesini çal
        PlaySound(attackSound, attackVolume);

        // Hasar değerini güncelle ve SwordHitbox'ı aktifleştir
        if (swordHitbox != null && swordHitboxComponent != null)
        {
            swordHitboxComponent.damage = attackDamage;
            swordHitbox.SetActive(true);
            StartCoroutine(DisableSwordHitbox());
        }
    }

    private IEnumerator DisableSwordHitbox()
    {
        yield return new WaitForSeconds(0.2f); // Saldırı hitbox'ın aktif kalma süresi
        if (swordHitbox != null)
        {
            swordHitbox.SetActive(false);
        }
    }

    // Ses çalmak için yardımcı metod
    private void PlaySound(AudioClip clip, float volume)
    {
        if (clip != null && effectsAudioSource != null)
        {
            effectsAudioSource.clip = clip;
            effectsAudioSource.volume = volume;
            effectsAudioSource.Play();
        }
    }

    #endregion

    #region Collision Check

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = true;
            if (animator != null) animator.SetBool(ANIM_IS_GROUNDED, true);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = false;
            if (animator != null) animator.SetBool(ANIM_IS_GROUNDED, false);

            // Zeminle temas kesildiğinde yürüme sesini durdur
            if (walkAudioSource.isPlaying)
            {
                walkAudioSource.Stop();
            }
        }
    }

    #endregion

    #region Health Management

    // Hasar alma metodu - PlayerHP tarafından çağrılır
    public void TakeDamage(int damage, Vector2 knockbackDirection)
    {
        if (isDead) return;

        // NOT: PlayerHP zaten hasarı uyguladı, burada sadece animasyon işlemleri yapılır

        // Hasar animasyonunu tetikle
        if (animator != null)
        {
            animator.SetTrigger(ANIM_HURT);
        }

        // Hasar sesini çal
        PlaySound(hurtSound, hurtVolume);

        // PlayerHP can kontrolü yaptı, burada tekrar kontrol etmeye gerek yok
    }


    #endregion

    #region Public Sound Volume Methods

    // Ses seviyelerini ayarlamak için public metotlar
    public void SetWalkSoundVolume(float volume)
    {
        walkVolume = Mathf.Clamp01(volume);
        if (walkAudioSource != null)
        {
            walkAudioSource.volume = walkVolume;
        }
    }

    public void SetAttackSoundVolume(float volume)
    {
        attackVolume = Mathf.Clamp01(volume);
    }

    public void SetJumpSoundVolume(float volume)
    {
        jumpVolume = Mathf.Clamp01(volume);
    }

    public void SetHurtSoundVolume(float volume)
    {
        hurtVolume = Mathf.Clamp01(volume);
    }

    #endregion

    #region Public Attack Properties

    // Hasar değerini değiştirmek için public metot
    public void SetAttackDamage(int newDamage)
    {
        attackDamage = Mathf.Max(0, newDamage); // Negatif değerleri engelle

        // SwordHitbox bileşeninin hasar değerini de güncelle
        if (swordHitboxComponent != null)
        {
            swordHitboxComponent.damage = attackDamage;
        }
    }

    // Hasar değerini getirmek için public metot
    public int GetAttackDamage()
    {
        return attackDamage;
    }

    #endregion
}

// Kılıç hitbox bileşeni
[System.Serializable]
public class KnightSwordHitbox : MonoBehaviour
{
    public int damage = 10;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            // KnightEnemy objesine hasar ver
            var enemy = collision.GetComponent<KnightEnemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log($"Düşmana {damage} hasar verildi!");
            }
        }
    }
}
