using UnityEngine;
using UnityEngine.InputSystem;

public class FrogMovement : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;

    public float moveSpeed = 5f; // Yürüme hızı
    public float jumpForce = 7f; // Zıplama gücü

    private bool isGrounded;

    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();

        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
    }

    void Update()
    {
        Move(); // Yürümeyi kontrol et
        Jump(); // Zıplamayı kontrol et
    }

    // Yürümeyi kontrol eden metod
    void Move()
    {
        // Yatay hareket girişini al
        float move = moveAction.ReadValue<Vector2>().x;

        // Yatay hız ile hareket et
        rb.linearVelocity = new Vector2(move * moveSpeed, rb.linearVelocity.y); // linearVelocity yerine velocity kullanıldı

        // Animator parametresini güncelle: Speed > 0 ise yürüyüş animasyonuna geç
        animator.SetFloat("Speed", Mathf.Abs(move));

        // Yön değiştirme (karakterin sağa veya sola bakması için)
        if (move != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(move), 1f, 1f); // Sağ/sol yönü
        }
    }

    // Zıplamayı kontrol eden metod
    void Jump()
    {
        if (isGrounded && jumpAction.triggered)
        {
            animator.SetTrigger("IsJumping"); // JumpTrigger'ı tetikle
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse); // Zıplama kuvvetini uygula
            isGrounded = false;
        }
    }

    // Zeminle temas kontrolü
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            animator.ResetTrigger("IsJumping"); // Zıplama bitince trigger'ı sıfırla
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}
