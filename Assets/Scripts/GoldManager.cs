using System.Collections;
using UnityEngine;

public class GoldManager : MonoBehaviour
{
    public GameObject goldPrefab; // Alt�n�n prefab�
    public Transform[] spawnPoints; // Alt�n�n belirece�i noktalar
    public float goldLifetime = 5f; // Alt�n�n ekranda kalma s�resi
    public float respawnTime = 10f; // Alt�n�n tekrar gelme s�resi

    void Start()
    {
        StartCoroutine(SpawnGoldRoutine());
    }

    public void SpawnGold()
    {
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject gold = Instantiate(goldPrefab, spawnPoint.position, Quaternion.identity);
        StartCoroutine(RemoveGoldAfterTime(gold, goldLifetime));
    }

    IEnumerator RemoveGoldAfterTime(GameObject gold, float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gold);
        StartCoroutine(RespawnGold(respawnTime));
    }

    IEnumerator SpawnGoldRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(respawnTime);
            SpawnGold();
        }
    }

    IEnumerator RespawnGold(float time)
    {
        yield return new WaitForSeconds(time);
        SpawnGold();
    }
}