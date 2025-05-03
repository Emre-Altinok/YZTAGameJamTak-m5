using System.Collections;
using UnityEngine;

public class GoldManager : MonoBehaviour
{
    public GameObject goldPrefab; // Alt�n�n prefab�
    public Transform[] spawnPoints; // Alt�n�n belirece�i noktalar
    public float goldLifetime = 30f; // Alt�n�n ekranda kalma s�resi
    public KeyCode spawnKey = KeyCode.E; // Alt�nlar� �a��rmak i�in kullan�lacak tu�

    private bool isGoldActive = false; // Alt�nlar�n aktif olup olmad���n� kontrol eder
    private bool isPlayerInTrigger = false; // Oyuncunun trigger i�inde olup olmad���n� kontrol eder

    void Update()
    {
        // Belirtilen tu�a bas�ld���nda ve oyuncu trigger i�indeyse alt�nlar� spawn et
        if (Input.GetKeyDown(spawnKey) && !isGoldActive && isPlayerInTrigger)
        {
            SpawnGold();
        }
    }

    public void SpawnGold()
    {
        isGoldActive = true; // Alt�nlar�n aktif oldu�unu i�aretle
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject gold = Instantiate(goldPrefab, spawnPoint.position, Quaternion.identity);
        StartCoroutine(RemoveGoldAfterTime(gold, goldLifetime));
    }

    IEnumerator RemoveGoldAfterTime(GameObject gold, float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gold);
        isGoldActive = false; // Alt�nlar�n art�k aktif olmad���n� i�aretle
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Oyuncu trigger alan�na girdi�inde
        if (collision.CompareTag("Player"))
        {
            isPlayerInTrigger = true;
            Debug.Log("Player entered the gold spawn area.");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Oyuncu trigger alan�ndan ��kt���nda
        if (collision.CompareTag("Player"))
        {
            isPlayerInTrigger = false;
            Debug.Log("Player exited the gold spawn area.");
        }
    }
}
