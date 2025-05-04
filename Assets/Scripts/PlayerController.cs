using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour
{
    // Bileşen referansları
    private Animator animator;
    private Rigidbody2D rb;
    private bool isDead = false; // Ölüm durumu (script içi kontrol için)

    // Hareket parametreleri
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;
    private bool isGrounded; // Zeminle temas durumu

    // Input System
    [SerializeField] private InputActionAsset inputActions;
    private InputAction moveAction, jumpAction, attackAction, deathAction;

    // Ses parametreleri
    [Header("Audio Settings")]
    [SerializeField] private AudioClip walkingSound; // Yürüme sesi
    [SerializeField] private AudioClip attackSound; // Saldırı sesi
    [SerializeField] private AudioClip hitSound; // Hasar alma sesi
    [SerializeField] private AudioClip deathSound; // Ölüm sesi
    [Range(0f, 1f)][SerializeField] private float walkVolume = 0.5f;
    [Range(0f, 1f)][SerializeField] private float attackVolume = 0.7f;
    [Range(0f, 1f)][SerializeField] private float hitVolume = 0.6f;
    [Range(0f, 1f)][SerializeField] private float deathVolume = 0.8f;
    private AudioSource walkAudioSource; // Yürüme sesi kaynağı
    private AudioSource effectsAudioSource; // Efekt sesleri kaynağı (saldırı, hasar, ölüm)

    // Animasyon parametreleri sabitleri
    private const string ANIM_IS_GROUNDED = "isGrounded";
    private const string ANIM_SPEED = "Speed";
    private const string ANIM_DEATH = "Death"; // Bool -> Trigger olarak değiştirildi
    private const string ANIM_JUMP = "Jump";
    private const string ANIM_ATTACK = "Attack";
    private const string ANIM_HIT = "Hit";

    // Coroutines
    private Coroutine hitboxDisableCoroutine;

    #region Unity Lifecycle Methods

    private void Awake()
    {
        SetupInputActions();
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();


        if (animator != null)
        {
            ResetAnimatorParameters();
        }
        else
        {
            Debug.LogError("Animator component not found on player!");
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
        isDead = true;

        // Ölüm animasyonunu trigger olarak tetikle
        if (animator != null) animator.SetTrigger(ANIM_DEATH);

        // Ölüm sesini çal
        PlaySound(deathSound, deathVolume);

        // Yürüme sesini durdur
        if (walkAudioSource.isPlaying)
        {
            walkAudioSource.Stop();
        }

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

        animator.ResetTrigger(ANIM_JUMP);
        animator.ResetTrigger(ANIM_ATTACK);
        animator.ResetTrigger(ANIM_HIT);
        animator.ResetTrigger(ANIM_DEATH); // Death artık trigger
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
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        // Zıplarken yürüme sesini durdur
        if (walkAudioSource.isPlaying)
        {
            walkAudioSource.Stop();
        }
    }

    private void Attack()
    {
        if (animator == null || isDead) return;
        Debug.Log("PlayerController: Saldırı başlatılıyor");
        // Saldırı animasyonunu tetikle
        animator.SetTrigger(ANIM_ATTACK);

        // Saldırı sesini çal
        PlaySound(attackSound, attackVolume);

    }

    // Hasar alma animasyonunu ve sesini tetikleyen metod
    public void TriggerHit()
    {
        if (animator != null && !isDead)
        {
            animator.SetTrigger(ANIM_HIT);
            PlaySound(hitSound, hitVolume);
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

    #region Public Ses Metotları

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

    public void SetHitSoundVolume(float volume)
    {
        hitVolume = Mathf.Clamp01(volume);
    }

    public void SetDeathSoundVolume(float volume)
    {
        deathVolume = Mathf.Clamp01(volume);
    }

    #endregion
}
