using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour
{
    // Bileşen referansları
    private Animator animator;
    private Rigidbody2D rb;
    private SwordHitbox swordHitbox; // SwordHitbox referansı
    private bool isDead = false; // Ölüm durumu

    // Hareket parametreleri
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;
    private bool isGrounded; // Zeminle temas durumu

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
        swordHitbox = GetComponentInChildren<SwordHitbox>(); // SwordHitbox referansını al

        if (animator != null)
        {
            ResetAnimatorParameters();
        }
        else
        {
            Debug.LogError("Animator component not found on player!");
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
            if (animator != null) animator.SetTrigger(ANIM_ATTACK);

            // Sword hitbox'ı etkinleştir
            if (swordHitbox != null)
            {
                swordHitbox.EnableHitbox();

                // Önceki coroutine varsa durdur
                if (hitboxDisableCoroutine != null)
                {
                    StopCoroutine(hitboxDisableCoroutine);
                }

                hitboxDisableCoroutine = StartCoroutine(DisableHitboxAfterAnimation());
            }
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
            transform.Translate(new Vector2(horizontal * moveSpeed * Time.deltaTime, 0));
            transform.localScale = new Vector3(horizontal > 0 ? 1 : -1, 1, 1);
        }
    }

    private void Jump()
    {
        if (rb == null || animator == null || !isGrounded) return;

        isGrounded = false;
        animator.SetTrigger(ANIM_JUMP);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); // linearVelocity yerine velocity kullan
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    #endregion

    #region Sword Hitbox Control

    private IEnumerator DisableHitboxAfterAnimation()
    {
        // Animasyonun tamamlanmasını bekle
        yield return new WaitForSeconds(0.5f); // Saldırı animasyon süresine göre ayarlayın
        if (swordHitbox != null)
        {
            swordHitbox.DisableHitbox();
        }
        hitboxDisableCoroutine = null;
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
        }
    }

    #endregion
}
