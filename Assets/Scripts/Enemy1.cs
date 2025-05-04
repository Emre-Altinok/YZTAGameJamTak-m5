using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy1 : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Attack Settings")]
    public float attackRange = 10f; // Sald�r� menzili
    public float attackCooldown = 3f; // Her 3 saniyede bir sald�r�
    public Transform player; // Oyuncu referans�
    public Transform firePoint; // Ate� noktas�

    [Header("Laser Settings")]
    public GameObject laserObject; // Lazer objesi (child object)
    public float laserDuration = 1.5f; // Lazerin aktif kalma s�resi
    public int laserDamage = 2; // Lazerin verdi�i hasar

    // Animasyon kontrol�
    private Animator animator;
    private bool isAttacking = false;
    private float lastAttackTime = 0f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;

        // Ba�lang��ta lazeri deaktif et
        if (laserObject != null)
        {
            laserObject.SetActive(false);

            // Lazerin collider'�n�n trigger oldu�undan emin ol
            Collider2D laserCollider = laserObject.GetComponent<Collider2D>();
            if (laserCollider != null)
            {
                laserCollider.isTrigger = true;
            }
        }

        // E�er player referans� atanmam��sa, Player tag'ine sahip objeyi bul
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

        // E�er sald�r� menzilindeyse ve sald�r� zaman� geldiyse sald�r
        if (distanceToPlayer <= attackRange && Time.time > lastAttackTime + attackCooldown)
        {
            StartCoroutine(Attack());
        }
    }

    private IEnumerator Attack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        // Sald�r� animasyonunu ba�lat
        animator.SetTrigger("Attack");

        // Animasyonun lazer ate�leme k�sm�na gelene kadar bekle (animasyon h�z�na g�re ayarla)
        yield return new WaitForSeconds(0.5f); // Bu s�reyi animasyonunuza g�re ayarlay�n

        // Lazeri aktifle�tir
        FireLaser();

        // Sald�r� tamamland�
        yield return new WaitForSeconds(1.0f); // Bu s�reyi animasyonunuza g�re ayarlay�n
        isAttacking = false;
    }

    // Animation Event'te �a�r�labilir
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

        // Hasar alma animasyonu oynat�labilir
        animator.SetTrigger("Hit");

        Debug.Log($"Enemy took damage! Health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // �l�m animasyonunu oynat
        animator.SetTrigger("Die");

        // T�m aktif coroutine'leri durdur
        StopAllCoroutines();

        // Collider'� devre d��� b�rak
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

        // D��man� yok et (animasyon bittikten sonra)
        Destroy(gameObject, 2f); // �l�m animasyonunun s�resine g�re ayarlay�n
    }

    // Mermi ile �arp��ma alg�lama
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            // Bullet script'inden hasar de�erini al
            Bullet bullet = other.GetComponent<Bullet>();
            if (bullet != null)
            {
                TakeDamage(bullet.damage);
            }
            else
            {
                // Bullet script'i yoksa varsay�lan 1 hasar ver
                TakeDamage(1);
            }

            // Mermiyi yok et
            Destroy(other.gameObject);
        }
    }
}

// Lazer davran���n� kontrol eden s�n�f
public class Laser : MonoBehaviour
{
    public int damage = 2; // Lazerin verdi�i hasar
    private HashSet<GameObject> damagedObjects = new HashSet<GameObject>(); // Hasar verilen objeler

    private void OnEnable()
    {
        // Lazer etkinle�tirildi�inde hasar listesini temizle
        damagedObjects.Clear();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Player �arp��ma kontrol�
        if (other.CompareTag("Player"))
        {
            // Bu objeye daha �nce hasar verilmemi� mi kontrol et
            if (!damagedObjects.Contains(other.gameObject))
            {
                // Hasar verildi olarak i�aretle
                damagedObjects.Add(other.gameObject);

                // Player'a hasar ver
                PlayerHP playerHP = other.GetComponent<PlayerHP>();
                if (playerHP != null)
                {
                    // Lazerden oyuncuya y�nde knockback uygula
                    Vector2 knockbackDirection = (other.transform.position - transform.position).normalized;
                    playerHP.TakeDamage(damage, knockbackDirection);
                    Debug.Log("Player hit by laser! Damage: " + damage);
                }
            }
        }
    }
}
