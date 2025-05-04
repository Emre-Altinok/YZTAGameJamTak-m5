using UnityEngine;
using UnityEngine.InputSystem;

public class KnightController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction attackAction;

    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private GameObject swordHitbox; // SwordHitbox referans�
    private bool isGrounded = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();

        // Input Actions
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        attackAction = playerInput.actions["Attack"];

        // SwordHitbox ba�lang��ta devre d���
        if (swordHitbox != null)
        {
            swordHitbox.SetActive(false);
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleJump();
        HandleAttack();
    }

    private void HandleMovement()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        float moveDirection = input.x;

        rb.linearVelocity = new Vector2(moveDirection * speed, rb.linearVelocity.y);

        // Animasyon parametresi
        animator.SetFloat("Speed", Mathf.Abs(moveDirection));

        // Karakterin y�n�n� de�i�tirme
        if (moveDirection > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (moveDirection < 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    private void HandleJump()
    {
        if (jumpAction.triggered && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isGrounded = false;

            // Animasyon parametresi
            animator.SetTrigger("Jump");
        }
    }

    private void HandleAttack()
    {
        if (attackAction.triggered)
        {
            // Animasyon parametresi
            animator.SetTrigger("Attack");

            // SwordHitbox'� aktif hale getir
            if (swordHitbox != null)
            {
                swordHitbox.SetActive(true);
            }

            // Sald�r� bitiminde SwordHitbox'� devre d��� b�rak
            Invoke(nameof(DisableSwordHitbox), 0.5f); // 0.5 saniye sonra devre d���
        }
    }

    private void DisableSwordHitbox()
    {
        if (swordHitbox != null)
        {
            swordHitbox.SetActive(false);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
}


public class KnightSwordHitbox : MonoBehaviour
{
    [SerializeField] private int damage = 10; // Verilecek hasar miktar�

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            // Enemy objesine hasar ver
            var enemy = collision.GetComponent<KnightEnemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
    }
}