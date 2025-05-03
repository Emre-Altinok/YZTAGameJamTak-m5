using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GoldManager : MonoBehaviour
{
    public GameObject goldPrefab; // Alt�n�n prefab�
    public Transform[] spawnPoints; // Alt�n�n belirece�i noktalar
    public float goldLifetime = 30f; // Alt�n�n ekranda kalma s�resi

    private bool isGoldActive = false; // Alt�nlar�n aktif olup olmad���n� kontrol eder

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Oyuncu trigger alan�na girdi�inde
        if (collision.CompareTag("Player") && !isGoldActive)
        {
            SpawnGold();
            Debug.Log("Player entered the gold spawn area. Gold spawned at all points.");
        }
    }

    public void SpawnGold()
    {
        Debug.Log("SpawnGold called. isGoldActive: " + isGoldActive);
        isGoldActive = true; // Alt�nlar�n aktif oldu�unu i�aretle

        // T�m spawn noktalar�nda alt�n olu�tur
        foreach (Transform spawnPoint in spawnPoints)
        {
            GameObject gold = Instantiate(goldPrefab, spawnPoint.position, Quaternion.identity);
            StartCoroutine(RemoveGoldAfterTime(gold, goldLifetime));
        }
    }

    IEnumerator RemoveGoldAfterTime(GameObject gold, float time)
    {
        Debug.Log("Gold will be removed after: " + time + " seconds.");
        yield return new WaitForSeconds(time);
        Destroy(gold);
        Debug.Log("Gold removed.");
    }
}
