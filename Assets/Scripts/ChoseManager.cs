using UnityEngine;

public class ChoseManager : MonoBehaviour
{
    [SerializeField] private GameObject panel1; // Ýlk panel
    [SerializeField] private GameObject panel2; // Ýkinci panel


    void Start()
    {
        // Panel baþlangýç durumlarý
        if (panel1 != null)
            panel1.SetActive(false);

        if (panel2 != null)
            panel2.SetActive(false);

    }

    // Ýlk paneli açan metod
    public void OpenPanel1()
    {
        if (panel1 != null)
            panel1.SetActive(true);
    }

    // Ýkinci paneli açan metod
    public void OpenPanel2()
    {
        if (panel2 != null)
            panel2.SetActive(true);
    }
}
