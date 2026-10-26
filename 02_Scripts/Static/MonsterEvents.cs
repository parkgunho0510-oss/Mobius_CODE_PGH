using System;
using UnityEngine;

public static class MonsterEvents
{
    public static event Action<GameObject, GameObject> MonsterReturnedEvent;

    public static void RaiseMonsterReturnedEvent(GameObject prefabKey, GameObject obj)
    {
        MonsterReturnedEvent?.Invoke(prefabKey, obj);
    }

    public static event Action MonsterDefeatedEvent;
    public static void RaiseMonsterDefeatedEvent()
    {
        MonsterDefeatedEvent?.Invoke();
    }
}