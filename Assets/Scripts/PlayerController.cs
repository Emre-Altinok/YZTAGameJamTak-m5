using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour
{
    // Bileþen referanslarý
    private Animator animator;
    private Rigidbody2D rb;

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

    #region Unity Lifecycle Methods

    private void Awake()
    {
        SetupInputActions();
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        ResetAnimatorParameters();
    }

    private void OnEnable()
    {
        EnableInputActions();
    }

    private void OnDisable()
    {
        DisableInputActions();
    }

    private void Update()
    {
        MoveCharacter();
    }

    #endregion

    #region Input System

    private void SetupInputActions()
    {
        moveAction = inputActions.FindAction("Move");
        jumpAction = inputActions.FindAction("Jump");
        attackAction = inputActions.FindAction("Attack");
        deathAction = inputActions.FindAction("Death");
    }

    private void EnableInputActions()
    {
        moveAction.Enable();

        jumpAction.Enable();
        jumpAction.performed += OnJump;

        attackAction.Enable();
        attackAction.performed += OnAttack;

        deathAction.Enable();
        deathAction.performed += OnDeath;
    }

    private void DisableInputActions()
    {
        moveAction.Disable();

        jumpAction.performed -= OnJump;
        jumpAction.Disable();

        attackAction.performed -= OnAttack;
        attackAction.Disable();

        deathAction.performed -= OnDeath;
        deathAction.Disable();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        Debug.Log("OnJumping1");
        Debug.Log("isGrounded: " + isGrounded);
        Debug.Log("context.performed: " + context.performed);
        // Sadece yerdeyse zýplayabilir
        if (context.performed && isGrounded)
        {
            Jump();
            Debug.Log("OnJumping");
        }
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            animator.SetTrigger(ANIM_ATTACK);
        }
    }

    private void OnDeath(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            animator.SetBool(ANIM_IS_DEAD, true);
        }
    }

    #endregion

    #region Character Control Methods

    private void ResetAnimatorParameters()
    {
        animator.SetBool(ANIM_IS_GROUNDED, true);
        animator.SetFloat(ANIM_SPEED, 0);
        animator.SetBool(ANIM_IS_DEAD, false);

        animator.ResetTrigger(ANIM_JUMP);
        animator.ResetTrigger(ANIM_ATTACK);
        animator.ResetTrigger(ANIM_HIT);
    }

    private void MoveCharacter()
    {
        float horizontal = moveAction.ReadValue<Vector2>().x;

        // Animatör parametresini güncelle
        animator.SetFloat(ANIM_SPEED, Mathf.Abs(horizontal));

        // Karakteri hareket ettir
        if (horizontal != 0)
        {
            transform.Translate(new Vector2(horizontal * moveSpeed * Time.deltaTime, 0));
            transform.localScale = new Vector3(horizontal > 0 ? 1 : -1, 1, 1);
        }
    }

    private void Jump()
    {
        // Zýplama iþlemini gerçekleþtir
        isGrounded = false; // Zýpladýktan sonra yere deðmiyor olarak ayarla
        animator.SetTrigger(ANIM_JUMP);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); // Mevcut y hýzýný sýfýrla
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        Debug.Log("Jumping");
    }

    #endregion

    #region Collision Check

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Zeminle temas kontrolü
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = true;
            animator.SetBool(ANIM_IS_GROUNDED, true);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // Zeminden ayrýlma kontrolü
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = false;
            animator.SetBool(ANIM_IS_GROUNDED, false);
        }
    }

    #endregion
}
