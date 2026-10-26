using System.Collections.Generic;
using UnityEngine;

public class BossCombatSelector : BTNode
{
    private BossBase _boss;
    private List<BTNode> _meleeAttacks;
    private List<BTNode> _rangedAttacks;
    private BTNode _currentAttack;

    public BossCombatSelector(BossBase boss, List<BTNode> melee, List<BTNode> ranged)
    {
        _boss = boss;
        _meleeAttacks = melee;
        _rangedAttacks = ranged;
    }

    public override NodeState Evaluate()
    {
        //  이미 진행 중인 공격이 있다면 끝날 때까지 무조건 유지
        if (_currentAttack != null)
        {
            NodeState state = _currentAttack.Evaluate();
            if (state != NodeState.Running)
            {
                _currentAttack = null; // 공격 완료
                return State = state;
            }
            return State = NodeState.Running;
        }

        // 진행 중인 공격이 없다면 현재 거리를 측정하여 단 1번만 패턴 결정
        float dist = _boss.DistanceToPlayer();
        
        if (dist <= _boss.AttackRange)
        {
            _currentAttack = _meleeAttacks[Random.Range(0, _meleeAttacks.Count)];
            return State = _currentAttack.Evaluate();
        }
        else if (dist >= _boss.AttackRange * 3f)
        {
            _currentAttack = _rangedAttacks[Random.Range(0, _rangedAttacks.Count)];
            return State = _currentAttack.Evaluate();
        }

        // 사거리 밖이면 공격 실패 반환 -> 부모 Selector가 추적(Chase) 노드로 넘어감
        return State = NodeState.Failure;
    }

    public override void Abort()
    {
        base.Abort();
        _currentAttack?.Abort();
        _currentAttack = null;
    }
}