using UnityEngine;

public class DungeonEnter : MonoBehaviour
{
    [SerializeField] private string SceneName = "Dungeon";

    private void Start()
    {
        if (DungeonSceneManager.Instance != null && DungeonSceneManager.Instance.IsReturningFromDungeon)
        {
            Debug.Log($"{gameObject.name} 입구가 비활성화되었습니다.");
            this.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            DungeonSceneManager.Instance.GoToScene(SceneName);
        }
    }
}