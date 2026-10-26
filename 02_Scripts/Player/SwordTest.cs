using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class SwordTest : MonoBehaviour
{
    public float damageAmount = 10f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            if (other.TryGetComponent<MonsterBase>(out MonsterBase enemy))
            {
                enemy.TakeDamage(damageAmount); 
            }
        }
    }
}
