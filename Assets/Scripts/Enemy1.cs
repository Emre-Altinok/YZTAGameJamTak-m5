using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy1 : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Attack Settings")]
    public float attackRange = 10f; // Saldýrý menzili
    public float attackCooldown = 3f; // Her 3 saniyede bir saldýrý
    public Transform player; // Oyuncu referansý
    public Transform firePoint; // Ateþ noktasý

    [Header("Laser Settings")]
    public GameObject laserObject; // Lazer objesi (child object)
    public float laserDuration = 1.5f; // Lazerin aktif kalma süresi
    public int laserDamage = 2; // Lazerin verdiði hasar

    // Animasyon kontrolü
    private Animator animator;
    private bool isAttacking = false;
    private float lastAttackTime = 0f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;

        // Baþlangýçta lazeri deaktif et
        if (laserObject != null)
        {
            laserObject.SetActive(false);

            // Lazerin collider'ýnýn trigger olduðundan emin ol
            Collider2D laserCollider = laserObject.GetComponent<Collider2D>();
            if (laserCollider != null)
            {
                laserCollider.isTrigger = true;
            }
        }

        // Eðer player referansý atanmamýþsa, Player tag'ine sahip objeyi bul
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }

    private void Update()
    {
        if (player == null || isAttacking) return;

        // Oyuncu ile aradaki mesafeyi kontrol et
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Eðer saldýrý menzilindeyse ve saldýrý zamaný geldiyse saldýr
        if (distanceToPlayer <= attackRange && Time.time > lastAttackTime + attackCooldown)
        {
            StartCoroutine(Attack());
        }
    }

    private IEnumerator Attack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        // Saldýrý animasyonunu baþlat
        animator.SetTrigger("Attack");

        // Animasyonun lazer ateþleme kýsmýna gelene kadar bekle (animasyon hýzýna göre ayarla)
        yield return new WaitForSeconds(0.5f); // Bu süreyi animasyonunuza göre ayarlayýn

        // Lazeri aktifleþtir
        FireLaser();

        // Saldýrý tamamlandý
        yield return new WaitForSeconds(1.0f); // Bu süreyi animasyonunuza göre ayarlayýn
        isAttacking = false;
    }

    // Animation Event'te çaðrýlabilir
    public void FireLaser()
    {
        if (laserObject != null)
        {
            // Lazere Laser scripti ekle veya al
            Laser laserScript = laserObject.GetComponent<Laser>();
            if (laserScript == null)
            {
                laserScript = laserObject.AddComponent<Laser>();
                laserScript.damage = laserDamage;
            }

            laserObject.SetActive(true);
            StartCoroutine(DeactivateLaser());
        }
    }

    private IEnumerator DeactivateLaser()
    {
        yield return new WaitForSeconds(laserDuration);
        if (laserObject != null)
        {
            laserObject.SetActive(false);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // Hasar alma animasyonu oynatýlabilir
        animator.SetTrigger("Hit");

        Debug.Log($"Enemy took damage! Health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Ölüm animasyonunu oynat
        animator.SetTrigger("Die");

        // Tüm aktif coroutine'leri durdur
        StopAllCoroutines();

        // Collider'ý devre dýþý býrak
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        // Lazeri kapat
        if (laserObject != null)
        {
            laserObject.SetActive(false);
        }

        // Düþmaný yok et (animasyon bittikten sonra)
        Destroy(gameObject, 2f); // Ölüm animasyonunun süresine göre ayarlayýn
    }

    // Mermi ile çarpýþma algýlama
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            // Bullet script'inden hasar deðerini al
            Bullet bullet = other.GetComponent<Bullet>();
            if (bullet != null)
            {
                TakeDamage(bullet.damage);
            }
            else
            {
                // Bullet script'i yoksa varsayýlan 1 hasar ver
                TakeDamage(1);
            }

            // Mermiyi yok et
            Destroy(other.gameObject);
        }
    }
}

// Lazer davranýþýný kontrol eden sýnýf
public class Laser : MonoBehaviour
{
    public int damage = 2; // Lazerin verdiði hasar
    private HashSet<GameObject> damagedObjects = new HashSet<GameObject>(); // Hasar verilen objeler

    private void OnEnable()
    {
        // Lazer etkinleþtirildiðinde hasar listesini temizle
        damagedObjects.Clear();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Player çarpýþma kontrolü
        if (other.CompareTag("Player"))
        {
            // Bu objeye daha önce hasar verilmemiþ mi kontrol et
            if (!damagedObjects.Contains(other.gameObject))
            {
                // Hasar verildi olarak iþaretle
                damagedObjects.Add(other.gameObject);

                // Player'a hasar ver
                PlayerHP playerHP = other.GetComponent<PlayerHP>();
                if (playerHP != null)
                {
                    // Lazerden oyuncuya yönde knockback uygula
                    Vector2 knockbackDirection = (other.transform.position - transform.position).normalized;
                    playerHP.TakeDamage(damage, knockbackDirection);
                    Debug.Log("Player hit by laser! Damage: " + damage);
                }
            }
        }
    }
}
