using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSwitch : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private CameraFollowManager cameraManager; // Kamera takip yöneticisi

    [Header("Ayarlar")]
    [Tooltip("Baþlangýçta aktif olacak karakter indeksi")]
    [SerializeField] private int startingPlayerIndex = 0; // Baþlangýçta aktif olacak karakter

    [Header("Durum Bilgisi")]
    [Tooltip("Þu anda aktif olan karakter indeksi")]
    [SerializeField] private int currentPlayerIndex; // Mevcut aktif oyuncu indeksi (SerializeField ile Inspector'da görünür)

    [Tooltip("Tüm oyuncu karakterleri (Otomatik doldurulur)")]
    [SerializeField] private GameObject[] playerCharacters; // Oyuncu karakterlerinin listesi

    private bool playerInTriggerArea = false; // Oyuncunun trigger alanýnda olup olmadýðý

    private void Awake()
    {
        // Eðer CameraFollowManager atanmadýysa bul
        if (cameraManager == null)
        {
            cameraManager = FindAnyObjectByType<CameraFollowManager>();
            if (cameraManager == null)
            {
                Debug.LogError("CameraFollowManager bulunamadý! Lütfen Inspector'dan atayýn.");
            }
        }
    }

    private void Start()
    {
        // Oyuncu karakterlerini al
        GetPlayerCharactersFromCameraManager();

        // Baþlangýç karakter indeksini ayarla
        currentPlayerIndex = startingPlayerIndex;

        // Karakterlerin aktiflik durumunu güncelle
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
            Debug.LogWarning("CameraFollowManager veya hedefler bulunamadý!");
        }
    }

    private void Update()
    {
        // Oyuncu trigger alanýndaysa ve E tuþuna basýldýysa
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

        // Çarpýþan nesne aktif oyuncu mu kontrol et
        if (collision.gameObject == playerCharacters[currentPlayerIndex])
        {
            playerInTriggerArea = true;
            Debug.Log("Oyuncu karakter switch alanýna girdi");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log("Trigger alanýndan çýkýldý: " + collision.gameObject.name);
        if (playerCharacters == null || currentPlayerIndex >= playerCharacters.Length)
            return;

        // Çarpýþan nesne aktif oyuncu mu kontrol et
        if (collision.gameObject == playerCharacters[currentPlayerIndex])
        {
            playerInTriggerArea = false;
            Debug.Log("Oyuncu karakter switch alanýndan çýktý");
        }
    }

    // Bu metodu diðer scriptlerden çaðýrabilirsiniz
    public void SwitchToNextPlayer()
    {
        if (playerCharacters == null || playerCharacters.Length <= 1 || cameraManager == null)
        {
            Debug.LogWarning("Geçiþ yapýlacak oyuncu karakteri yok veya CameraFollowManager bulunamadý!");
            return;
        }

        // Mevcut karakteri saða taþý
        MoveCurrentPlayerAside();

        // Mevcut karakteri deaktif et
        playerCharacters[currentPlayerIndex].SetActive(false);

        // Sonraki karaktere geç
        currentPlayerIndex = (currentPlayerIndex + 1) % playerCharacters.Length;

        // Yeni aktif karakteri etkinleþtir
        playerCharacters[currentPlayerIndex].SetActive(true);

        // Kamerayý deðiþtir
        cameraManager.ChangePlayer();

        Debug.Log("Oyuncu " + currentPlayerIndex + " aktif edildi");
    }
    // Mevcut oyuncuyu saða taþýyan yeni metod
    private void MoveCurrentPlayerAside()
    {
        if (playerCharacters == null || currentPlayerIndex >= playerCharacters.Length)
            return;

        GameObject currentPlayer = playerCharacters[currentPlayerIndex];
        if (currentPlayer != null)
        {
            // Mevcut karakterin pozisyonunu al
            Vector3 currentPosition = currentPlayer.transform.position;

            // Karakteri X ekseninde 10 birim saða taþý
            currentPlayer.transform.position = new Vector3(
                currentPosition.x + 10f, // X ekseninde 10 birim saða taþý
                currentPosition.y,
                currentPosition.z
            );

            Debug.Log("Mevcut karakter saða taþýndý: " + currentPlayer.name);
        }
    }

    // Bu metodu diðer scriptlerden çaðýrabilirsiniz
    public void SwitchToPlayer(int playerIndex)
    {
        if (playerCharacters == null || playerCharacters.Length <= 1 || cameraManager == null)
        {
            Debug.LogWarning("Geçiþ yapýlacak oyuncu karakteri yok veya CameraFollowManager bulunamadý!");
            return;
        }

        if (playerIndex < 0 || playerIndex >= playerCharacters.Length)
        {
            Debug.LogWarning("Geçersiz oyuncu indeksi: " + playerIndex);
            return;
        }

        // Mevcut karakteri saða taþý
        MoveCurrentPlayerAside();

        // Mevcut karakteri deaktif et
        playerCharacters[currentPlayerIndex].SetActive(false);

        // Belirtilen karaktere geç
        currentPlayerIndex = playerIndex;

        // Yeni aktif karakteri etkinleþtir
        playerCharacters[currentPlayerIndex].SetActive(true);

        // Kamerayý deðiþtir
        cameraManager.ChangePlayer();

        Debug.Log("Oyuncu " + currentPlayerIndex + " aktif edildi");
    }

    // UpdatePlayerActiveStates metodu artýk sadece baþlangýç durumunda kullanýlacak
    private void UpdatePlayerActiveStates()
    {
        // Tüm karakterlerin aktiflik durumlarýný güncelle (baþlangýç için)
        for (int i = 0; i < playerCharacters.Length; i++)
        {
            if (playerCharacters[i] != null)
            {
                playerCharacters[i].SetActive(i == currentPlayerIndex);
            }
        }
    }

    // Aktif oyuncu GameObject'ini döndüren özellik
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
