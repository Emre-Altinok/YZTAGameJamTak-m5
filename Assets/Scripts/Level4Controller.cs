using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Level4Controller : MonoBehaviour
{
    // Bile�en referanslar�
    private Animator animator;
    private Rigidbody2D rb;
    private bool isActive = true;

    // Hareket parametreleri
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    private bool isGrounded;

    // Ses parametreleri
    [Header("Audio Settings")]
    [SerializeField] private AudioClip walkingSound;
    [SerializeField] private AudioClip jumpSound;
    [Range(0f, 1f)]
    [SerializeField] private float walkVolume = 0.7f;
    [Range(0f, 1f)]
    [SerializeField] private float jumpVolume = 0.7f;
    private AudioSource walkAudioSource;
    private AudioSource effectsAudioSource;

    // Input System
    [Header("Input System")]
    [SerializeField] private InputActionAsset inputActions;
    private InputAction moveAction, jumpAction;

    // Animasyon parametreleri sabitleri
    private const string ANIM_IS_GROUNDED = "isGrounded";
    private const string ANIM_SPEED = "Speed";
    private const string ANIM_JUMP = "Jump";

    #region Unity Lifecycle Methods

    private void Awake()
    {
        SetupInputActions();

        // Debug kontrolü ekle
        if (inputActions != null)
            Debug.Log("InputActions bulundu: " + inputActions.name);
        else
            Debug.LogError("InputActions bulunamadı!");
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // AudioSource bile�enlerini ayarla
        SetupAudioSources();

        if (animator != null)
        {
            ResetAnimatorParameters();
        }
        else
        {
            Debug.LogWarning("Animator component not found on Level4Controller!");
        }

        DisableInputActions(); // Önce devre dışı bırak
        EnableInputActions();  // Sonra yeniden etkinleştir

        Debug.Log("Input aksiyonları yeniden etkinleştirildi");

    }

    private void SetupAudioSources()
    {
        // Y�r�me i�in AudioSource
        walkAudioSource = GetComponent<AudioSource>();
        if (walkAudioSource == null)
        {
            walkAudioSource = gameObject.AddComponent<AudioSource>();
        }

        // Y�r�me sesi ayarlar�
        if (walkingSound != null)
        {
            walkAudioSource.clip = walkingSound;
            walkAudioSource.loop = true;
            walkAudioSource.playOnAwake = false;
            walkAudioSource.volume = walkVolume;
        }

        // Efektler i�in ikinci AudioSource (z�plama sesi i�in)
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
        // Controller aktifse hareket etmesine izin ver
        if (isActive)
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

        try
        {
            moveAction = inputActions.FindAction("Move");
            jumpAction = inputActions.FindAction("Jump");

            // Aksiyon kontrolleri
            if (moveAction == null)
                Debug.LogError("Move action bulunamad�! InputActionAsset i�inde 'Move' aksiyonu tan�ml� m�?");

            if (jumpAction == null)
                Debug.LogError("Jump action bulunamad�! InputActionAsset i�inde 'Jump' aksiyonu tan�ml� m�?");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Input actions ayarlan�rken hata olu�tu: {e.Message}");
        }
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
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded && isActive)
        {
            Jump();
        }
    }

    #endregion

    #region Character Control Methods

    private void ResetAnimatorParameters()
    {
        if (animator == null) return;

        animator.SetBool(ANIM_IS_GROUNDED, true);
        animator.SetFloat(ANIM_SPEED, 0);
        animator.ResetTrigger(ANIM_JUMP);
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

            // Y�r�me sesini �al
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

        // Z�plama sesini �al
        PlaySound(jumpSound, jumpVolume);

        // Z�plarken y�r�me sesini durdur
        if (walkAudioSource.isPlaying)
        {
            walkAudioSource.Stop();
        }
    }

    // Ses �almak i�in yard�mc� metod
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

            // Zeminle temas kesildi�inde y�r�me sesini durdur
            if (walkAudioSource.isPlaying)
            {
                walkAudioSource.Stop();
            }
        }
    }

    #endregion

    #region Public Control Methods

    // D��ar�dan kontrol� etkinle�tirmek/devre d��� b�rakmak i�in
    public void SetActive(bool active)
    {
        isActive = active;

        if (!isActive)
        {
            // Hareketi durdur
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }

            // Animasyonu durdur
            if (animator != null)
            {
                animator.SetFloat(ANIM_SPEED, 0);
            }

            // Y�r�me sesini durdur
            if (walkAudioSource != null && walkAudioSource.isPlaying)
            {
                walkAudioSource.Stop();
            }

            // Input aksiyonlar�n� devre d��� b�rak
            DisableInputActions();
        }
        else
        {
            // Input aksiyonlar�n� etkinle�tir
            EnableInputActions();
        }
    }

    // Ses seviyelerini ayarlamak i�in public metotlar
    public void SetWalkSoundVolume(float volume)
    {
        walkVolume = Mathf.Clamp01(volume);
        if (walkAudioSource != null)
        {
            walkAudioSource.volume = walkVolume;
        }
    }

    public void SetJumpSoundVolume(float volume)
    {
        jumpVolume = Mathf.Clamp01(volume);
    }

    #endregion
}
