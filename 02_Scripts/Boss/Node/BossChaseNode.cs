using UnityEngine;

public class BossChaseNode : BTNode
{
    private BossBase _boss;
    private float _attackRangeMultiplier;

    // attackRangeMultiplier: 무기/공격 사거리 밖에서 멈출 거리 비율 (기본 3배)
    public BossChaseNode(BossBase boss, float attackRangeMultiplier = 3f)
    {
        _boss = boss;
        _attackRangeMultiplier = attackRangeMultiplier;
    }

    public override NodeState Evaluate()
    {
        // 예외: 플레이어가 죽었으면 추적 실패 반환
        if (_boss.IsPlayerDead()) return State = NodeState.Failure;

        float dist = _boss.DistanceToPlayer();

        // 목표 사거리 안에 들어오면 추적 성공(Success) 반환 -> 공격 노드로 넘어감
        if (dist <= _boss.AttackRange * _attackRangeMultiplier)
        {
            _boss.Agent.isStopped = true;
            return State = NodeState.Success;
        }

        // 아직 멀었으면 계속 추적(Running)
        _boss.Agent.isStopped = false;
        _boss.Agent.SetDestination(_boss.Player.transform.position);
        _boss.RotateToTarget();

        return State = NodeState.Running;
    }

    public override void Abort()
    {
        base.Abort();
        _boss.Agent.isStopped = true;
    }
}