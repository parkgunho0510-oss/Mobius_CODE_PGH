using System.Collections;
using UnityEngine;

public class MeleeAttackNode : MonsterAttackNode
{
    private Monster_Skeleton_Scout _scout;

    public MeleeAttackNode(Monster_Skeleton_Scout scout) : base(scout)
    {
        _scout = scout;
        _delay = 1.5f; // 대기 시간 설정
    }

    protected override IEnumerator Co_ExecuteAttack()
    {
        _scout.RotateToTarget(5);
        yield return new WaitForSeconds(0.2f);

        _scout.Animator.SetTrigger("Attack");
        _scout.Combat.SetActiveHitboxGroup("MeleeCollider");

    }

    public override void Abort()
    {
        base.Abort();
        if (_scout.Combat != null) _scout.Combat.StopCasting();
    }
}