using UnityEngine;

public class MonsterChaseNode : BTNode
{
    private MonsterBase _monster;

    public MonsterChaseNode(MonsterBase monster)
    {
        _monster = monster;
    }

    public override NodeState Evaluate()
    {
        // 플레이어가 죽었거나 타겟이 없으면 추적 실패
        if (_monster.IsPlayerDead()) return State = NodeState.Failure;

        float dist = _monster.DistanceToPlayer();

        // 거리가 사거리 안으로 들어왔다면 추적 완료(Success)
        if (dist <= _monster.AttackRange)
        {
            _monster.Agent.isStopped = true;
            _monster.Animator.SetBool("IsMove", false);
            return State = NodeState.Success; 
        }

        // 거리가 너무 멀어지면 추적 포기(Failure) -> FSM이 Idle로 넘기게 됨
        if (dist > _monster.DetectRange)
        {
            return State = NodeState.Failure;
        }

        // 추적 진행 (Running)
        _monster.Agent.isStopped = false;
        _monster.Animator.SetBool("IsMove", true);
        _monster.RotateToTarget();
        _monster.Agent.SetDestination(_monster.Player.transform.position);

        return State = NodeState.Running;
    }
}