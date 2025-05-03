using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GoldManager : MonoBehaviour
{
    public GameObject goldPrefab; // Altýnýn prefabý
    public Transform[] spawnPoints; // Altýnýn belireceði noktalar
    public float goldLifetime = 30f; // Altýnýn ekranda kalma süresi

    private bool isGoldActive = false; // Altýnlarýn aktif olup olmadýðýný kontrol eder

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Oyuncu trigger alanýna girdiðinde
        if (collision.CompareTag("Player") && !isGoldActive)
        {
            SpawnGold();
            Debug.Log("Player entered the gold spawn area. Gold spawned at all points.");
        }
    }

    public void SpawnGold()
    {
        Debug.Log("SpawnGold called. isGoldActive: " + isGoldActive);
        isGoldActive = true; // Altýnlarýn aktif olduðunu iþaretle

        // Tüm spawn noktalarýnda altýn oluþtur
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
