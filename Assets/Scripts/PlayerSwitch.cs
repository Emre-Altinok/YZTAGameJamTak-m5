using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSwitch : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private CameraFollowManager cameraManager; // Kamera takip y�neticisi

    [Header("Ayarlar")]
    [Tooltip("Ba�lang��ta aktif olacak karakter indeksi")]
    [SerializeField] private int startingPlayerIndex = 0; // Ba�lang��ta aktif olacak karakter

    [Header("Durum Bilgisi")]
    [Tooltip("�u anda aktif olan karakter indeksi")]
    [SerializeField] private int currentPlayerIndex; // Mevcut aktif oyuncu indeksi (SerializeField ile Inspector'da g�r�n�r)

    [Tooltip("T�m oyuncu karakterleri (Otomatik doldurulur)")]
    [SerializeField] private GameObject[] playerCharacters; // Oyuncu karakterlerinin listesi

    private bool playerInTriggerArea = false; // Oyuncunun trigger alan�nda olup olmad���

    private void Awake()
    {
        // E�er CameraFollowManager atanmad�ysa bul
        if (cameraManager == null)
        {
            cameraManager = FindAnyObjectByType<CameraFollowManager>();
            if (cameraManager == null)
            {
                Debug.LogError("CameraFollowManager bulunamad�! L�tfen Inspector'dan atay�n.");
            }
        }
    }

    private void Start()
    {
        // Oyuncu karakterlerini al
        GetPlayerCharactersFromCameraManager();

        // Ba�lang�� karakter indeksini ayarla
        currentPlayerIndex = startingPlayerIndex;

        // Karakterlerin aktiflik durumunu g�ncelle
        UpdatePlayerActiveStates();
    }

    private void GetPlayerCharactersFromCameraManager()
    {
        if (cameraManager != null && cameraManager.targets != null && cameraManager.targets.Length > 0)
        {
            Transform[] targets = cameraManager.targets;
            playerCharacters = new GameObject[targets.Length];

            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] != null)
                {
                    playerCharacters[i] = targets[i].gameObject;
                }
            }

            Debug.Log($"{playerCharacters.Length} karakter bulundu.");
        }
        else
        {
            Debug.LogWarning("CameraFollowManager veya hedefler bulunamad�!");
        }
    }

    private void Update()
    {
        // Oyuncu trigger alan�ndaysa ve E tu�una bas�ld�ysa
        if (playerInTriggerArea && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            SwitchToNextPlayer();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Trigger entered by: " + collision.gameObject.name);
        if (playerCharacters == null || currentPlayerIndex >= playerCharacters.Length)
            return;

        // �arp��an nesne aktif oyuncu mu kontrol et
        if (collision.gameObject == playerCharacters[currentPlayerIndex])
        {
            playerInTriggerArea = true;
            Debug.Log("Oyuncu karakter switch alan�na girdi");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log("Trigger alan�ndan ��k�ld�: " + collision.gameObject.name);
        if (playerCharacters == null || currentPlayerIndex >= playerCharacters.Length)
            return;

        // �arp��an nesne aktif oyuncu mu kontrol et
        if (collision.gameObject == playerCharacters[currentPlayerIndex])
        {
            playerInTriggerArea = false;
            Debug.Log("Oyuncu karakter switch alan�ndan ��kt�");
        }
    }

    // Bu metodu di�er scriptlerden �a��rabilirsiniz
    public void SwitchToNextPlayer()
    {
        if (playerCharacters == null || playerCharacters.Length <= 1 || cameraManager == null)
        {
            Debug.LogWarning("Ge�i� yap�lacak oyuncu karakteri yok veya CameraFollowManager bulunamad�!");
            return;
        }

        // Mevcut karakteri sa�a ta��
        MoveCurrentPlayerAside();

        // Mevcut karakteri deaktif et
        playerCharacters[currentPlayerIndex].SetActive(false);

        // Sonraki karaktere ge�
        currentPlayerIndex = (currentPlayerIndex + 1) % playerCharacters.Length;

        // Yeni aktif karakteri etkinle�tir
        playerCharacters[currentPlayerIndex].SetActive(true);

        // Kameray� de�i�tir
        cameraManager.ChangePlayer();

        Debug.Log("Oyuncu " + currentPlayerIndex + " aktif edildi");
    }
    // Mevcut oyuncuyu sa�a ta��yan yeni metod
    private void MoveCurrentPlayerAside()
    {
        if (playerCharacters == null || currentPlayerIndex >= playerCharacters.Length)
            return;

        GameObject currentPlayer = playerCharacters[currentPlayerIndex];
        if (currentPlayer != null)
        {
            // Mevcut karakterin pozisyonunu al
            Vector3 currentPosition = currentPlayer.transform.position;

            // Karakteri X ekseninde 10 birim sa�a ta��
            currentPlayer.transform.position = new Vector3(
                currentPosition.x + 10f, // X ekseninde 10 birim sa�a ta��
                currentPosition.y,
                currentPosition.z
            );

            Debug.Log("Mevcut karakter sa�a ta��nd�: " + currentPlayer.name);
        }
    }

    // Bu metodu di�er scriptlerden �a��rabilirsiniz
    public void SwitchToPlayer(int playerIndex)
    {
        if (playerCharacters == null || playerCharacters.Length <= 1 || cameraManager == null)
        {
            Debug.LogWarning("Ge�i� yap�lacak oyuncu karakteri yok veya CameraFollowManager bulunamad�!");
            return;
        }

        if (playerIndex < 0 || playerIndex >= playerCharacters.Length)
        {
            Debug.LogWarning("Ge�ersiz oyuncu indeksi: " + playerIndex);
            return;
        }

        // Mevcut karakteri sa�a ta��
        MoveCurrentPlayerAside();

        // Mevcut karakteri deaktif et
        playerCharacters[currentPlayerIndex].SetActive(false);

        // Belirtilen karaktere ge�
        currentPlayerIndex = playerIndex;

        // Yeni aktif karakteri etkinle�tir
        playerCharacters[currentPlayerIndex].SetActive(true);

        // Kameray� de�i�tir
        cameraManager.ChangePlayer();

        Debug.Log("Oyuncu " + currentPlayerIndex + " aktif edildi");
    }

    // UpdatePlayerActiveStates metodu art�k sadece ba�lang�� durumunda kullan�lacak
    private void UpdatePlayerActiveStates()
    {
        // T�m karakterlerin aktiflik durumlar�n� g�ncelle (ba�lang�� i�in)
        for (int i = 0; i < playerCharacters.Length; i++)
        {
            if (playerCharacters[i] != null)
            {
                playerCharacters[i].SetActive(i == currentPlayerIndex);
            }
        }
    }

    // Aktif oyuncu GameObject'ini d�nd�ren �zellik
    public GameObject ActivePlayer
    {
        get
        {
            if (playerCharacters != null && currentPlayerIndex >= 0 && currentPlayerIndex < playerCharacters.Length)
            {
                return playerCharacters[currentPlayerIndex];
            }
            return null;
        }
    }
}
