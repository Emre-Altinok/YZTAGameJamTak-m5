using UnityEngine;
using UnityEngine.InputSystem;

public class frogjump : MonoBehaviour
{
    public float ziplamaGucu = 7f;
    public float yatayGuc = 5f;
    public LayerMask zeminKatmani;

    private Rigidbody2D rb;
    private bool zemindeMi = false;
    private Vector2 hareketInput;

    private PlayerInput playerInput;
    private InputAction hareketAction;
    private InputAction ziplamaAction;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();

        hareketAction = playerInput.actions["Move"];
        ziplamaAction = playerInput.actions["Jump"];
    }

    void Update()
    {
        hareketInput = hareketAction.ReadValue<Vector2>();

        if (zemindeMi && ziplamaAction.triggered)
        {
            Vector2 ziplaYon = new Vector2(hareketInput.x * yatayGuc, ziplamaGucu);
            rb.linearVelocity = ziplaYon;
            zemindeMi = false;
        }
    }

    void OnCollisionEnter2D(Collision2D temas)
    {
        if (((1 << temas.gameObject.layer) & zeminKatmani) != 0)
        {
            zemindeMi = true;
        }
    }

    void OnCollisionExit2D(Collision2D temas)
    {
        if (((1 << temas.gameObject.layer) & zeminKatmani) != 0)
        {
            zemindeMi = false;
        }
    }
}
