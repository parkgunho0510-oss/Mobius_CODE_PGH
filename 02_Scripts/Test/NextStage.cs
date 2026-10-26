using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

public class NextStage : MonoBehaviour
{
    [SerializeField] private string scene;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    // Update is called once per frame
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            SceneManager.LoadScene(scene);
        }
    }
}
