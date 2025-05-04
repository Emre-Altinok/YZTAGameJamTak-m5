using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 1;
    public float lifetime = 30f; // Mermi kaç saniye sonra yok olacak
    public Vector2 direction = Vector2.right; // Varsayılan sağa doğru hareket
    public bool debugMode = false; // Debug modu

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f; // Yerçekimi etkisi olmasın
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // Yüksek hızlar için sürekli çarpışma kontrolü
            rb.interpolation = RigidbodyInterpolation2D.Interpolate; // Daha yumuşak hareket
        }
        else
        {
            // Mevcut Rigidbody2D'nin yerçekiminden etkilenmediğinden emin ol
            rb.gravityScale = 0f;
        }

        // Belirli bir süre sonra mermiyi yok et
        Destroy(gameObject, lifetime);

        if (debugMode)
            Debug.Log($"Bullet created. Will destroy in {lifetime} seconds.");
    }

    private void Start()
    {
        // Mermiyi belirlenen yönde hareket ettir
        rb.linearVelocity = direction.normalized * speed;

        // Hareket yönüne doğru dön
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        if (debugMode)
            Debug.Log($"Bullet started moving. Direction: {direction}, Speed: {speed}");
    }

    // Merminin yönünü dışarıdan ayarlama
    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;

        // Yeni yönde hareket ettir
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;

            // Hareket yönüne doğru dön
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            if (debugMode)
                Debug.Log($"Bullet direction changed. New direction: {direction}");
        }
    }

    // Bullet'ın hızını dışarıdan ayarlama
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
        if (rb != null)
        {
            rb.linearVelocity = direction.normalized * speed;
        }
    }

    private void OnDestroy()
    {
        if (debugMode)
            Debug.Log("Bullet destroyed at: " + transform.position);
    }

    // Duvarlara veya diğer nesnelere çarptığında yok olma davranışı
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Çarpışma bilgisini logla
        if (debugMode)
            Debug.Log("Bullet collided with: " + collision.gameObject.name + " (Tag: " + collision.gameObject.tag + ")");

        // Eğer düşman değilse ve Ignore tag'i yoksa yok ol
        if (!collision.gameObject.CompareTag("Enemy") && !collision.gameObject.CompareTag("Ignore") && !collision.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }

    // Alternatif olarak, Trigger kullanabiliriz
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Trigger çarpışma kontrolü de ekleyebiliriz
        if (debugMode)
            Debug.Log("Bullet trigger with: " + other.gameObject.name + " (Tag: " + other.gameObject.tag + ")");

        // Enemy için özel davranış - Enemy1.cs kendisi hasar kaydediyor
        if (other.CompareTag("Enemy"))
        {
            // Vuruş gerçekleşti, ancak yok etme işlemi düşman tarafında
            if (debugMode)
                Debug.Log("Hit enemy: " + other.gameObject.name);
        }
        // Diğer engellerde yok ol (zemin, duvar vb.)
        else if (!other.CompareTag("Player") && !other.CompareTag("Ignore") && !other.CompareTag("Bullet"))
        {
            Destroy(gameObject);
        }
    }
}

