using System.Collections;
using UnityEngine;

public class GoldManager : MonoBehaviour
{
    public GameObject goldPrefab; // Altýnýn prefabý
    public Transform[] spawnPoints; // Altýnýn belireceði noktalar
    public float goldLifetime = 30f; // Altýnýn ekranda kalma süresi
    public KeyCode spawnKey = KeyCode.E; // Altýnlarý çaðýrmak için kullanýlacak tuþ

    private bool isGoldActive = false; // Altýnlarýn aktif olup olmadýðýný kontrol eder
    private bool isPlayerInTrigger = false; // Oyuncunun trigger içinde olup olmadýðýný kontrol eder

    void Update()
    {
        // Belirtilen tuþa basýldýðýnda ve oyuncu trigger içindeyse altýnlarý spawn et
        if (Input.GetKeyDown(spawnKey) && !isGoldActive && isPlayerInTrigger)
        {
            SpawnGold();
        }
    }

    public void SpawnGold()
    {
        isGoldActive = true; // Altýnlarýn aktif olduðunu iþaretle
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject gold = Instantiate(goldPrefab, spawnPoint.position, Quaternion.identity);
        StartCoroutine(RemoveGoldAfterTime(gold, goldLifetime));
    }

    IEnumerator RemoveGoldAfterTime(GameObject gold, float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gold);
        isGoldActive = false; // Altýnlarýn artýk aktif olmadýðýný iþaretle
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Oyuncu trigger alanýna girdiðinde
        if (collision.CompareTag("Player"))
        {
            isPlayerInTrigger = true;
            Debug.Log("Player entered the gold spawn area.");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Oyuncu trigger alanýndan çýktýðýnda
        if (collision.CompareTag("Player"))
        {
            isPlayerInTrigger = false;
            Debug.Log("Player exited the gold spawn area.");
        }
    }
}
