using UnityEngine;
using UnityEngine.InputSystem;
public class SpaceController : MonoBehaviour
{
    // Bile�en referanslar�
    private Animator animator;
    private Rigidbody2D rb;
    private PlayerHP playerHP;

    // Hareket parametreleri
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;
    private bool isGrounded; // Zeminle temas durumu

    // Ses parametreleri
    [Header("Audio Settings")]
    [SerializeField] private AudioClip walkingSound; // Yürüme sesi
    [Range(0f, 1f)]
    [SerializeField] private float walkVolume = 0.7f; // Yürüme sesi seviyesi
    [Tooltip("Ses seviyesi: 0 = sessiz, 1 = maksimum")]
    private AudioSource audioSource; // Ses kaynağı

    // Silah parametreleri
    [Header("Weapon Settings")]
    [SerializeField] private GameObject bulletPrefab; // Mermi prefab'i
    [SerializeField] private Transform firePoint; // Ate� edilecek nokta
    [SerializeField] private float fireRate = 0.25f; // Ate� etme h�z� (saniye ba��na at��)
    private float lastFireTime = 0f; // Son ate� zaman�
    private bool isDead = false; // �l�m durumu

    // Input System
    [SerializeField] private InputActionAsset inputActions;
    private InputAction moveAction, jumpAction, attackAction, deathAction;

    // Animasyon parametreleri sabitleri
    private const string ANIM_IS_GROUNDED = "isGrounded";
    private const string ANIM_SPEED = "Speed";
    private const string ANIM_IS_DEAD = "isDead";
    private const string ANIM_JUMP = "Jump";
    private const string ANIM_ATTACK = "Attack";
    private const string ANIM_HIT = "Hit";

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

        // AudioSource bileşenini al veya ekle
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Yürüme sesi ayarları
        if (walkingSound != null)
        {
            audioSource.clip = walkingSound;
            audioSource.loop = true; // Yürüme sesi sürekli çalacak
            audioSource.playOnAwake = false; // Başlangıçta çalmayacak
            audioSource.volume = walkVolume; // Ses seviyesini ayarla
        }

        if (animator != null)
        {
            ResetAnimatorParameters();
        }
        else
        {
            Debug.LogError("Animator component not found on player!");
        }

        if (firePoint == null)
        {
            // E�er firePoint atanmam��sa, varsay�lan olarak karakter d�n�� y�n�nde olu�tur
            firePoint = transform;
            Debug.LogWarning("FirePoint not assigned, using player transform instead!");
        }
    }

    // Yürüme sesi seviyesini değiştirmek için public metod
    public void SetWalkSoundVolume(float volume)
    {
        walkVolume = Mathf.Clamp01(volume); // 0-1 arasında sınırla
        if (audioSource != null)
        {
            audioSource.volume = walkVolume;
        }
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
        // Oyuncu �lm��se hareket etmesin
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
            FireBullet();
        }
    }

    private void OnDeath(InputAction.CallbackContext context)
    {
        if (context.performed && !isDead)
        {
            SetDeathState();
        }
    }

    // D��ar�dan �l�m tetiklenebilmesi i�in public metod
    public void SetDeathState()
    {
        isDead = true;
        if (animator != null) animator.SetBool(ANIM_IS_DEAD, true);

        // Hareket etmeyi durdur
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    #endregion

    #region Character Control Methods

    private void ResetAnimatorParameters()
    {
        if (animator == null) return;

        animator.SetBool(ANIM_IS_GROUNDED, true);
        animator.SetFloat(ANIM_SPEED, 0);
        animator.SetBool(ANIM_IS_DEAD, false);

        animator.ResetTrigger(ANIM_JUMP);
        animator.ResetTrigger(ANIM_ATTACK);
        animator.ResetTrigger(ANIM_HIT);
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
            if (!audioSource.isPlaying && isGrounded)
            {
                audioSource.Play();
            }
        }
        else
        {
            // Karakter duruyor
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }


    private void Jump()
    {
        if (rb == null || animator == null || !isGrounded) return;

        isGrounded = false;
        animator.SetTrigger(ANIM_JUMP);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        // Zıplarken yürüme sesini durdur
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    // Silah ate�leme metodu
    private void FireBullet()
    {
        // Ate� h�z� kontrol�
        if (Time.time < lastFireTime + (1f / fireRate)) return;

        lastFireTime = Time.time;

        // Ate� animasyonunu tetikle
        if (animator != null) animator.SetTrigger(ANIM_ATTACK);

        // Mermi prefab kontrol�
        if (bulletPrefab == null)
        {
            Debug.LogError("Bullet prefab is not assigned!");
            return;
        }

        // Karakterin bakt��� y�ne g�re ate� etme
        Vector2 direction = transform.localScale.x > 0 ? Vector2.right : Vector2.left;

        // Mermiyi olu�tur
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Bullet bulletComponent = bullet.GetComponent<Bullet>();

        if (bulletComponent != null)
        {
            bulletComponent.SetDirection(direction);
        }
        else
        {
            // E�er Bullet componenti yoksa, varsay�lan hareket ile ilerlet
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
            {
                bulletRb.linearVelocity = direction * 10f;
            }
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
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }

        }
    }

    // Hasar alma metodu - PlayerHP taraf�ndan �a�r�labilir
    public void OnDamageReceived()
    {
        if (animator != null && !isDead)
        {
            animator.SetTrigger(ANIM_HIT);
        }
    }

    #endregion
}
