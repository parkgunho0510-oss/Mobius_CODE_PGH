using System.Collections;
using UnityEngine;

public class RangedAttackNode : MonsterAttackNode
{
    private Monster_Skeleton_Archer _archer;

    public RangedAttackNode(Monster_Skeleton_Archer archer) : base(archer)
    {
        _archer = archer;
        _delay = 2.0f; //  시간 설정
    }

    protected override IEnumerator Co_ExecuteAttack()
    {
        _archer.RotateToTarget();
        _archer.Animator.SetTrigger("Attack");

        yield break;
    }

    public override void Abort()
    {
        base.Abort();
        var combat = _archer.GetComponent<MonsterCombat>();
        if (combat != null) combat.StopCasting();
    }
}