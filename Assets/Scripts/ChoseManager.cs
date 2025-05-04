using UnityEngine;

public class ChoseManager : MonoBehaviour
{
    [SerializeField] private GameObject panel1; // �lk panel
    [SerializeField] private GameObject panel2; // �kinci panel


    void Start()
    {
        // Panel ba�lang�� durumlar�
        if (panel1 != null)
            panel1.SetActive(false);

        if (panel2 != null)
            panel2.SetActive(false);

    }

    // �lk paneli a�an metod
    public void OpenPanel1()
    {
        if (panel1 != null)
            panel1.SetActive(true);
    }

    // �kinci paneli a�an metod
    public void OpenPanel2()
    {
        if (panel2 != null)
            panel2.SetActive(true);
    }
}
