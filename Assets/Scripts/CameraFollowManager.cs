using Unity.Cinemachine;
using UnityEngine;

public class CameraFollowManager : MonoBehaviour
{
    public CinemachineCamera vcam;
    public Transform[] targets;
    private int currentIndex = 0;


        public void ChangePlayer()
    {

            currentIndex = (currentIndex + 1) % targets.Length;
            vcam.Follow = targets[currentIndex];

    }

}
