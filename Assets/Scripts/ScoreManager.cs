using UnityEngine;
using TMPro;  // TextMeshPro'yu kullanmak için gerekli namespace

public class ScoreManager : MonoBehaviour
{
    public float score = 0f;         // Skor değişkeni
    public TMP_Text scoreText;       // Skoru gösterecek olan TextMeshPro UI öğesi

    // Skoru ekleyerek güncelleme fonksiyonu
    public void ScoreAdd(float points)
    {
        score += points;            // Skora verilen puanı ekle
        UpdateScoreDisplay();        // Skor ekranını güncelle
    }

    // Skoru UI'da güncelleme fonksiyonu
    private void UpdateScoreDisplay()
    {
        scoreText.text = "Score: " + score.ToString("F2");  // Skoru iki ondalıklı sayı formatında göster
    }
}