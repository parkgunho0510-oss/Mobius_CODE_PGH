using System;
using UnityEngine;

public static class CombatEventBus
{
    // 공격을 시작/종료했는지 식별하기 위한 이벤트
    public static event Action OnAttackBeginEvent;
    public static event Action OnAttackEndEvent;

    public static void RaiseAttackBegin()
    {
        OnAttackBeginEvent?.Invoke();
    }

    public static void RaiseAttackEnd()
    {
        OnAttackEndEvent?.Invoke();
    }
}