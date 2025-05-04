using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 1;
    public float lifetime = 3f; // Mermi ka� saniye sonra yok olacak
    public Vector2 direction = Vector2.right; // Varsay�lan sa�a do�ru hareket

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Belirli bir s�re sonra mermiyi yok et
        Destroy(gameObject, lifetime);
    }

    private void Start()
    {
        // Mermiyi belirlenen y�nde hareket ettir
        rb.linearVelocity = direction.normalized * speed;

        // Hareket y�n�ne do�ru d�n
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    // Merminin y�n�n� d��ar�dan ayarlama
    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;

        // Yeni y�nde hareket ettir
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;

            // Hareket y�n�ne do�ru d�n
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    // Duvarlara veya di�er nesnelere �arpt���nda yok olabilir
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // E�er d��man de�ilse yok ol (d��manla �arp��ma OnTriggerEnter2D ile Enemy taraf�ndan kontrol ediliyor)
        if (!collision.gameObject.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }
}

