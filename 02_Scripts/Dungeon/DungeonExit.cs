using UnityEngine;

public class DungeonExit : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            DungeonSceneManager.Instance.ReturnToScene();
        }
    }
}