using UnityEngine;

public class button : MonoBehaviour
{
    public Animator buttonAnimator;
    private bool basildi = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!basildi && other.CompareTag("Player"))
        {
            basildi = true;
            if (buttonAnimator != null)
            {
                buttonAnimator.SetTrigger("Pressed");
            }
        }
    }
}
