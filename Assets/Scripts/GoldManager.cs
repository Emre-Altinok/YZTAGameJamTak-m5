using System.Collections;
using UnityEngine;

public class GoldManager : MonoBehaviour
{
    public GameObject goldPrefab; // Altýnýn prefabý
    public Transform[] spawnPoints; // Altýnýn belireceði noktalar
    public float goldLifetime = 5f; // Altýnýn ekranda kalma süresi
    public float respawnTime = 10f; // Altýnýn tekrar gelme süresi

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