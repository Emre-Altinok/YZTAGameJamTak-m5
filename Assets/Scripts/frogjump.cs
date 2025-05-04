using UnityEngine;
using UnityEngine.InputSystem;

public class frogjump : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public float ziplamaGucu = 7f;
    public float yatayGuc = 5f;
    public LayerMask zeminKatmani;

    [Header("Animasyon Ayarları")]
    public float yonDegisimHizi = 10f; // Karakterin dönüş hızı

    [Header("Input Ayarları")]
    [SerializeField] private InputActionAsset inputActions; // Input Action Asset referansı

    private Rigidbody2D rb;
    private bool zemindeMi = false;
    private Vector2 hareketInput;
    private float hareketYonu = 1f; // 1 = sağ, -1 = sol

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // Input System
    private InputAction hareketAction;
    private InputAction ziplamaAction;

    // Animatör parametreleri
    private static readonly int IsJumping = Animator.StringToHash("isJumping");
    private static readonly int IsGrounded = Animator.StringToHash("isGrounded");
    private static readonly int HorizontalSpeed = Animator.StringToHash("horizontalSpeed");

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D bulunamadı! Lütfen objeye ekleyin.");
            this.enabled = false;
            return;
        }

        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Input Actions
        if (inputActions == null)
        {
            Debug.LogError("Input Actions asset atanmamış! Lütfen Inspector'dan atayın.");
            this.enabled = false;
            return;
        }

        SetupInputActions();

        // LayerMask kontrolü
        if (zeminKatmani.value == 0)
        {
            Debug.LogWarning("Zemin katmanı ayarlanmamış! Lütfen Inspector'dan 'zeminKatmani' değişkenini ayarlayın.");
        }
    }

    private void SetupInputActions()
    {
        try
        {
            var actionMap = inputActions.FindActionMap("Player");
            if (actionMap != null)
            {
                hareketAction = actionMap.FindAction("Move");
                ziplamaAction = actionMap.FindAction("Jump");

                hareketAction.Enable();
                ziplamaAction.Enable();
            }
            else
            {
                Debug.LogError("'Player' action map bulunamadı!");
                this.enabled = false;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Input actions yüklenirken hata: " + e.Message);
            this.enabled = false;
        }
    }

    private void OnEnable()
    {
        if (hareketAction != null) hareketAction.Enable();
        if (ziplamaAction != null) ziplamaAction.Enable();
    }

    private void OnDisable()
    {
        if (hareketAction != null) hareketAction.Disable();
        if (ziplamaAction != null) ziplamaAction.Disable();
    }

    void Update()
    {
        if (hareketAction == null || ziplamaAction == null) return;

        // Hareket inputunu oku
        hareketInput = hareketAction.ReadValue<Vector2>();

        // Yön belirleme - ÖNEMLİ DEĞİŞİKLİK
        UpdateFacingDirection();

        // Animasyon parametrelerini güncelle
        if (animator != null)
        {
            animator.SetBool(IsGrounded, zemindeMi);
            animator.SetFloat(HorizontalSpeed, Mathf.Abs(hareketInput.x));
        }

        // Zıplama kontrolü
        if (zemindeMi && ziplamaAction.triggered)
        {
            Jump();
        }
    }

    // YENİ METOD: Yüz yönünü ve hareket yönünü güncelle
    private void UpdateFacingDirection()
    {
        // Yatay hareket inputu varsa
        if (Mathf.Abs(hareketInput.x) > 0.1f)
        {
            // Yeni hareket yönünü belirle (1 = sağ, -1 = sol)
            hareketYonu = Mathf.Sign(hareketInput.x);

            // Sprite'ı hareket yönüne göre çevir
            if (spriteRenderer != null)
            {
                // A tuşu (sol) için flipX = true, D tuşu (sağ) için flipX = false
                spriteRenderer.flipX = (hareketYonu < 0);
            }
        }
    }

    void Jump()
    {
        // Zıplama yönünü ve gücünü hesapla - mevcut yöne göre zıpla
        Vector2 ziplaYon = new Vector2(hareketYonu * yatayGuc, ziplamaGucu);

        // Rigidbody'nin hızını ayarla
        rb.linearVelocity = ziplaYon;

        // Zemin durumunu güncelle
        zemindeMi = false;

        // Zıplama animasyonunu tetikle
        if (animator != null)
        {
            animator.SetBool(IsJumping, true);
            animator.SetBool(IsGrounded, false);
        }
    }

    void OnCollisionEnter2D(Collision2D temas)
    {
        if (((1 << temas.gameObject.layer) & zeminKatmani.value) != 0)
        {
            zemindeMi = true;

            // Yere indiğinde zıplama animasyonunu sonlandır
            if (animator != null)
            {
                animator.SetBool(IsJumping, false);
                animator.SetBool(IsGrounded, true);
            }
        }
    }

    void OnCollisionExit2D(Collision2D temas)
    {
        if (((1 << temas.gameObject.layer) & zeminKatmani.value) != 0)
        {
            zemindeMi = false;

            if (animator != null)
            {
                animator.SetBool(IsGrounded, false);
            }
        }
    }
}
